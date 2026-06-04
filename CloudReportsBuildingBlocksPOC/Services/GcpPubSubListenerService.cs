namespace CloudReportsBuildingBlocksPOC.Services;

using CloudReportsBuildingBlocksPOC.Configuration;
using CloudReportsBuildingBlocksPOC.Services.Abstractions;
using Google.Api.Gax;
using Google.Cloud.PubSub.V1;
using Grpc.Core;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Collections.Concurrent;
using System.Text.Json;

/// <summary>
/// GCP listener service.
/// </summary>
public partial class GcpPubSubListenerService : IPubSubListenerService, IDisposable
{
    private readonly string _projectId;
    private readonly ILogger<GcpPubSubListenerService> _logger;
    private readonly PubSubOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ResiliencePipeline _resiliencePipeline;
    private readonly ConcurrentDictionary<string, SubscriberClient> _subscribers = new();
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _cancellationTokens = new();
    private bool _disposed;

    public GcpPubSubListenerService(
        IOptions<PubSubOptions> options,
        ILogger<GcpPubSubListenerService> logger)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options.Value;
        _projectId = _options.ProjectId;
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
        };

        _resiliencePipeline = BuildResiliencePipeline(_options);
    }

    public async Task StartAsync<T>(
        string subscriptionId,
        Func<T, IDictionary<string, string>, CancellationToken, Task<bool>> messageHandler,
        CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(subscriptionId);
        ArgumentNullException.ThrowIfNull(messageHandler);

        if (_subscribers.ContainsKey(subscriptionId))
        {
            LogSubscriptionStatus(subscriptionId, "already active");
            return;
        }

        try
        {
            var subscriptionName = SubscriptionName.FromProjectSubscription(_projectId, subscriptionId);

            var subscriberClientBuilder = new SubscriberClientBuilder
            {
                SubscriptionName = subscriptionName,
                Settings = new SubscriberClient.Settings
                {
                    AckDeadline = TimeSpan.FromSeconds(_options.Subscriber.AckDeadlineSeconds),
                    AckExtensionWindow = TimeSpan.FromSeconds(_options.Subscriber.MaxAckExtensionSeconds),
                    FlowControlSettings = new FlowControlSettings(
                        maxOutstandingElementCount: _options.Subscriber.MaxConcurrentMessages,
                        maxOutstandingByteCount: null),
                },
            };

            var subscriber = await subscriberClientBuilder.BuildAsync(cancellationToken);

            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _cancellationTokens[subscriptionId] = cts;

            // Start listening
            var startTask = subscriber.StartAsync(async (message, ct) =>
            {
                return await ProcessMessageAsync(message, messageHandler, ct);
            });

            _subscribers[subscriptionId] = subscriber;

            LogSubscriptionStatus(subscriptionId, $"started listening for message type {typeof(T).Name}");

            // Keep the task running
            _ = Task.Run(
            async () =>
            {
                try
                {
                    await startTask;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in subscription {SubscriptionId} listener task {Message}", subscriptionId, ex.Message);
                }
            }, cts.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start subscription listener for {SubscriptionId} {Message}", subscriptionId, ex.Message);
            throw;
        }
    }

    public async Task StartAsync(
        string subscriptionId,
        Func<byte[], IDictionary<string, string>, CancellationToken, Task<bool>> messageHandler,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(subscriptionId);
        ArgumentNullException.ThrowIfNull(messageHandler);

        if (_subscribers.ContainsKey(subscriptionId))
        {
            LogSubscriptionStatus(subscriptionId, "already active");
            return;
        }

        try
        {
            var subscriptionName = SubscriptionName.FromProjectSubscription(_projectId, subscriptionId);

            var subscriberClientBuilder = new SubscriberClientBuilder
            {
                SubscriptionName = subscriptionName,
                Settings = new SubscriberClient.Settings
                {
                    AckDeadline = TimeSpan.FromSeconds(_options.Subscriber.AckDeadlineSeconds),
                    AckExtensionWindow = TimeSpan.FromSeconds(_options.Subscriber.MaxAckExtensionSeconds),
                    FlowControlSettings = new FlowControlSettings(
                        maxOutstandingElementCount: _options.Subscriber.MaxConcurrentMessages,
                        maxOutstandingByteCount: null),
                },
            };

            var subscriber = await subscriberClientBuilder.BuildAsync(cancellationToken);

            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _cancellationTokens[subscriptionId] = cts;

            // Start listening
            var startTask = subscriber.StartAsync(async (message, ct) =>
            {
                return await ProcessRawMessageAsync(message, messageHandler, ct);
            });

            _subscribers[subscriptionId] = subscriber;

            LogSubscriptionStatus(subscriptionId, "started listening forraw messages");

            // Keep the task running
            _ = Task.Run(
            async () =>
            {
                try
                {
                    await startTask;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in subscription {SubscriptionId} listener task {Message}", subscriptionId, ex.Message);
                }
            }, cts.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start subscription listener for {SubscriptionId} listener task {Message}", subscriptionId, ex.Message);
        }
    }

    private async Task<SubscriberClient.Reply> ProcessMessageAsync<T>(
        PubsubMessage message,
        Func<T, IDictionary<string, string>, CancellationToken, Task<bool>> messageHandler,
        CancellationToken cancellationToken)
        where T : class
    {
        var messageId = message.MessageId;
        var attributes = message.Attributes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        try
        {
            var result = await _resiliencePipeline.ExecuteAsync(
            async ct =>
            {
                try
                {
                    var json = message.Data.ToStringUtf8();
                    var entity = JsonSerializer.Deserialize<T>(json, _jsonOptions);

                    if (entity == null)
                    {
                        LogMessageProcessFailed("unknown", typeof(T).Name, "null payload");
                        return false;
                    }

                    LogMessageProcess(messageId);
                    var success = await messageHandler(entity, attributes, ct);

                    if (success)
                    {
                        LogMessageProcessSuccess(messageId, typeof(T).Name);
                    }
                    else
                    {
                        LogMessageProcessFailed(messageId, typeof(T).Name, "handler returned false");
                    }

                    return success;
                }
                catch (JsonException ex)
                {
                    LogMessageProcessFailed(messageId, typeof(T).Name, ex.Message);
                    return false;
                }
                catch (Exception ex)
                {
                    LogMessageProcessFailed(messageId, typeof(T).Name, ex.Message);
                    return false;
                }
            }, cancellationToken);

            return result ? SubscriberClient.Reply.Ack : SubscriberClient.Reply.Nack;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error processing message {MessageId} after retries {Message}", messageId, ex.Message);
            // After all retries failed, Nack to requeue
            return SubscriberClient.Reply.Nack;
        }
    }

    private async Task<SubscriberClient.Reply> ProcessRawMessageAsync(
        PubsubMessage message,
        Func<byte[], IDictionary<string, string>, CancellationToken, Task<bool>> messageHandler,
        CancellationToken cancellationToken)
    {
        var messageId = message.MessageId;
        var attributes = message.Attributes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        try
        {
            var result = await _resiliencePipeline.ExecuteAsync(
            async ct =>
            {
                try
                {
                    var data = message.Data.ToByteArray();

                    LogMessageProcess(messageId);

                    var success = await messageHandler(data, attributes, ct);

                    if (success)
                    {
                        LogMessageProcessSuccess(messageId, "raw");
                    }
                    else
                    {
                        LogMessageProcessFailed(messageId, "raw", "Handler returned false");
                    }

                    return success;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing raw message {MessageId} {Message}", messageId, ex.Message);
                    return false;
                }
            }, cancellationToken);

            return result ? SubscriberClient.Reply.Ack : SubscriberClient.Reply.Nack;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error processing raw message {MessageId} after retries {Message}", messageId, ex.Message);

            return SubscriberClient.Reply.Nack;
        }
    }

    public async Task StopAsync(string subscriptionId, CancellationToken cancellationToken = default)
    {
        if (!_subscribers.TryRemove(subscriptionId, out var subscriber))
        {
            LogSubscriptionStatus(subscriptionId, "not found");
            return;
        }

        try
        {
            if (_cancellationTokens.TryRemove(subscriptionId, out var cts))
            {
                await cts.CancelAsync();
                cts.Dispose();
            }

            await subscriber.StopAsync(cancellationToken);

            LogSubscriptionStatus(subscriptionId, "stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping subscription {SubscriptionId} {Message}", subscriptionId, ex.Message);
        }
    }

    public async Task StopAllAsync(CancellationToken cancellationToken = default)
    {
        LogSubscriptionStatus("all", "stopping active subscriptions");

        var stopTasks = _subscribers.Keys.Select(subscriptionId => 
            StopAsync(subscriptionId, cancellationToken)).ToList();

        await Task.WhenAll(stopTasks);

        LogSubscriptionStatus("all", "stopped");
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        foreach (var cts in _cancellationTokens.Values)
        {
            try
            {
                cts.Cancel();
                cts.Dispose();
            }
            catch (Exception ex)
            {
                LogWarning($"Error disposing cancellation token {ex.Message}");
            }
        }

        foreach (var subscriber in _subscribers.Values)
        {
            try
            {
                subscriber.StopAsync(CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                LogWarning($"Error stopping subscriber during dispose {ex.Message}");
            }
        }

        _subscribers.Clear();
        _cancellationTokens.Clear();
        _disposed = true;

        GC.SuppressFinalize(this);
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
            ShouldHandle = new PredicateBuilder()
                .Handle<RpcException>(ex =>
                {
                    return ex.StatusCode == StatusCode.Unavailable ||
                           ex.StatusCode == StatusCode.DeadlineExceeded ||
                           ex.StatusCode == StatusCode.Internal ||
                           ex.StatusCode == StatusCode.ResourceExhausted ||
                           ex.StatusCode == StatusCode.Aborted;
                })
                .Handle<TimeoutException>()
                .Handle<JsonException>(),
            OnRetry = args =>
            {
                LogWarning($"Message processing retry attempt {args.AttemptNumber} after {args.RetryDelay.TotalMilliseconds}ms due to: {args.Outcome.Exception?.Message}");

                return ValueTask.CompletedTask;
            },
        };

        var circuitBreakerOptions = new CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            MinimumThroughput = options.CircuitBreaker.FailureThreshold,
            SamplingDuration = TimeSpan.FromSeconds(options.CircuitBreaker.SamplingDurationSeconds),
            BreakDuration = TimeSpan.FromSeconds(options.CircuitBreaker.DurationOfBreakSeconds),
            ShouldHandle = new PredicateBuilder().Handle<Exception>(),
            OnOpened = args =>
            {
                _logger.LogError("Circuit breaker opened for message processing. Will retry after {BreakDuration}", args.BreakDuration);

                return ValueTask.CompletedTask;
            },
            OnClosed = args =>
            {
                LogWarning("Circuit breaker closed. Message processing resumed.");
                return ValueTask.CompletedTask;
            },
            OnHalfOpened = args =>
            {
                LogWarning("Circuit breaker half-opened. Testing message processing.");
                return ValueTask.CompletedTask;
            },
        };

        return new ResiliencePipelineBuilder()
            .AddRetry(retryOptions)
            .AddCircuitBreaker(circuitBreakerOptions)
            .Build();
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "{message}")]
    partial void LogWarning(string message);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Subscription {subscriptionId} {status}")]
    partial void LogSubscriptionStatus(string subscriptionId, string status);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Processing message {messageId}")]
    partial void LogMessageProcess(string messageId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Message process failed for message Id {messageId} of type {type} {fail}")]
    partial void LogMessageProcessFailed(string messageId, string type, string fail);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Message process succeded for message Id {messageId} of type {type}")]
    partial void LogMessageProcessSuccess(string messageId, string type);
}
