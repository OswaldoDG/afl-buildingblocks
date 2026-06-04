namespace CloudReportsBuildingBlocksPOC.BackgroundServices;

using CloudReportsBuildingBlocksPOC.Configuration;
using CloudReportsBuildingBlocksPOC.Models.Events;
using CloudReportsBuildingBlocksPOC.Services.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

/// <summary>
/// GCP Pub/Sub listener for file events. Listens to the specified subscription and processes incoming messages related to file events, such as batch completions.
/// </summary>
/// <param name="listenerService">The service responsible for listening to Pub/Sub messages.</param>
/// <param name="logger">Logger instance.</param>
/// <param name="options">Configuration options for Pub/Sub.</param>
public partial class FileEventsListenerBackgroundService(IPubSubListenerService listenerService, 
        ILogger<FileEventsListenerBackgroundService> logger,
        IOptions<PubSubOptions> options) : BackgroundService
{
    private readonly string _subscriptionName = options.Value.FileEventsSubscription;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogStartListener();

        try
        {
            await listenerService.StartAsync<BatchCompletedEvent>(
                _subscriptionName,
                async (message, attributes, ct) =>
                {
                    try
                    {
                        LogMessageReceived(message.Id, message.Items.Count);

                        // Process the order event
                        await ProcessOrderEventAsync(message, ct);

                        return true; // Acknowledge message
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error processing order event {Message}", ex.Message);

                        // Nack - will retry
                        return false;
                    }
                },
                stoppingToken);

            // Keep running until cancellation
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            LogStopListener();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fatal error in Order Events Listener: {Message}", ex.Message);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        LogStopListener();
        await listenerService.StopAllAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }

    private async Task ProcessOrderEventAsync(BatchCompletedEvent fileEvent, CancellationToken ct)
    {
        // Your business logic here
        await Task.Delay(100, ct); // Simulate processing
        LogProcessOrderEventAsync(fileEvent.Id);
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Received event for batch: {id}, File count: {count}")]
    partial void LogMessageReceived(long id, int count);

    [LoggerMessage(Level = LogLevel.Information, Message = "Order Events Listener start")]
    partial void LogStartListener();

    [LoggerMessage(Level = LogLevel.Information, Message = "Order Events Listener stopping")]
    partial void LogStopListener();

    [LoggerMessage(Level = LogLevel.Information, Message = "Order {id} processed successfully")]
    partial void LogProcessOrderEventAsync(long id);

}