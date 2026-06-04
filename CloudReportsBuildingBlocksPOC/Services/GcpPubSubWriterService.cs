namespace CloudReportsBuildingBlocksPOC.Services;

using CloudReportsBuildingBlocksPOC.Configuration;
using CloudReportsBuildingBlocksPOC.Services.Abstractions;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Text.Json;

/// <summary>
/// GCP Pub /Sub writer service. Provides functionality to publish messages to Pub/Sub topics 
/// with retry and circuit breaker policies for resilience.
/// </summary>
public partial class GcpPubSubWriterService : IPubSubWriterService, IDisposable
{
    private readonly string _projectId;
    private readonly ILogger<GcpPubSubWriterService> _logger;
    private readonly Dictionary<string, PublisherClient> _publishers = [];
    private readonly SemaphoreSlim _publisherLock = new (1, 1);
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ResiliencePipeline _resiliencePipeline;
    private bool _disposed;

    public GcpPubSubWriterService(
        ILogger<GcpPubSubWriterService> logger,
        IOptions<PubSubOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _projectId = options.Value.ProjectId;
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
        };

        _resiliencePipeline = BuildResiliencePipeline(options.Value);
    }

    public async Task<string> PublishMessageAsync<T>(string topicId, T message, CancellationToken cancellationToken = default)
        where T : class
    {
        return await PublishMessageAsync(topicId, message, null, cancellationToken);
    }

    public async Task<string> PublishMessageAsync<T>(
        string topicId,
        T message,
        IDictionary<string, string>? attributes = null,
        CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(topicId);
        ArgumentNullException.ThrowIfNull(message);

        return await _resiliencePipeline.ExecuteAsync(
        async ct =>
        {
            try
            {
                var publisher = await GetOrCreatePublisherAsync(topicId, ct);

                var json = JsonSerializer.Serialize(message, _jsonOptions);
                var data = ByteString.CopyFromUtf8(json);

                var pubsubMessage = new PubsubMessage
                {
                    Data = data,
                    OrderingKey = string.Empty,
                };

                // Add attributes if provided
                if (attributes != null)
                {
                    foreach (var kvp in attributes)
                    {
                        pubsubMessage.Attributes[kvp.Key] = kvp.Value;
                    }
                }

                // Add default attributes
                pubsubMessage.Attributes["messageType"] = typeof(T).Name;
                pubsubMessage.Attributes["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

                var messageId = await publisher.PublishAsync(pubsubMessage);

                LogMessagePublished(topicId, messageId, typeof(T).Name);

                return messageId;
            }
            catch (Exception ex) when (ex is not RpcException)
            {
                _logger.LogError(ex, "Error publishing message to topic {TopicId}. MessageType: {Type} Message: {Message}", topicId, typeof(T).Name, ex.Message);
                return string.Empty;
            }
        }, cancellationToken);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        foreach (var publisher in _publishers.Values)
        {
            try
            {
                publisher.ShutdownAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error shutting down publisher. Message: {Message}", ex.Message);
            }
        }

        _publishers.Clear();
        _publisherLock.Dispose();
        _disposed = true;

        GC.SuppressFinalize(this);
    }

    private async Task<PublisherClient> GetOrCreatePublisherAsync(string topicId, CancellationToken cancellationToken)
    {
        if (_publishers.TryGetValue(topicId, out var existingPublisher))
        {
            return existingPublisher;
        }

        await _publisherLock.WaitAsync(cancellationToken);
        try
        {
            // Double-check after acquiring lock
            if (_publishers.TryGetValue(topicId, out existingPublisher))
            {
                return existingPublisher;
            }

            var topicName = TopicName.FromProjectTopic(_projectId, topicId);
            var publisher = await PublisherClient.CreateAsync(topicName);

            _publishers[topicId] = publisher;

            LogNewPublisher(topicId);

            return publisher;
        }
        finally
        {
            _publisherLock.Release();
        }
    }

    private ResiliencePipeline BuildResiliencePipeline(PubSubOptions options)
    {
        var retryOptions = new RetryStrategyOptions
        {
            MaxRetryAttempts = options.Retry.MaxRetryAttempts,
            Delay = TimeSpan.FromMilliseconds(options.Retry.InitialDelayMilliseconds),
            BackoffType = DelayBackoffType.Exponential,
            MaxDelay = TimeSpan.FromSeconds(options.Retry.MaxDelaySeconds),
            UseJitter = true,
            ShouldHandle = new PredicateBuilder().Handle<RpcException>(ex =>
            {
                // Retry on transient gRPC errors
                return ex.StatusCode == StatusCode.Unavailable ||
                       ex.StatusCode == StatusCode.DeadlineExceeded ||
                       ex.StatusCode == StatusCode.Internal ||
                       ex.StatusCode == StatusCode.ResourceExhausted ||
                       ex.StatusCode == StatusCode.Aborted;
            })
            .Handle<TimeoutException>(),
            OnRetry = args =>
            {
                LogResilienceWarning($"Retry attempt {args.AttemptNumber} after {args.RetryDelay.TotalMilliseconds}ms due to: {args.Outcome.Exception?.Message}");

                return ValueTask.CompletedTask;
            },
        };

        var circuitBreakerOptions = new CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            MinimumThroughput = options.CircuitBreaker.FailureThreshold,
            SamplingDuration = TimeSpan.FromSeconds(options.CircuitBreaker.SamplingDurationSeconds),
            BreakDuration = TimeSpan.FromSeconds(options.CircuitBreaker.DurationOfBreakSeconds),
            ShouldHandle = new PredicateBuilder().Handle<RpcException>(),
            OnOpened = args =>
            {
                LogResilienceWarning($"Circuit breaker opened due to failures. Will retry after {args.BreakDuration}");

                return ValueTask.CompletedTask;
            },
            OnClosed = args =>
            {
                LogResilienceWarning("Circuit breaker closed. Normal operations resumed.");
                return ValueTask.CompletedTask;
            },
            OnHalfOpened = args =>
            {
                LogResilienceWarning("Circuit breaker half-opened. Testing if service recovered.");
                return ValueTask.CompletedTask;
            },
        };

        return new ResiliencePipelineBuilder()
            .AddRetry(retryOptions)
            .AddCircuitBreaker(circuitBreakerOptions)
            .Build();
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "{message}")]
    partial void LogResilienceWarning(string message);

    [LoggerMessage(Level = LogLevel.Information, Message = "Created new publisher for topic {topicId}")]
    partial void LogNewPublisher(string topicId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Message published successfully to topic {topicId}. MessageId: {messageId}, Type: {messageType}")]
    partial void LogMessagePublished(string topicId, string messageId, string messageType);
}
