namespace CloudReportsBuildingBlocksPOC.Services.Abstractions;

public interface IPubSubListenerService
{
    /// <summary>
    /// Starts listening to a subscription with a message handler.
    /// </summary>
    Task StartAsync<T>(
        string subscriptionId, 
        Func<T, IDictionary<string, string>, CancellationToken, Task<bool>> messageHandler,
        CancellationToken cancellationToken = default) 
        where T : class;

    /// <summary>
    /// Starts listening to a subscription with a raw message handler.
    /// </summary>
    Task StartAsync(
        string subscriptionId,
        Func<byte[], IDictionary<string, string>, CancellationToken, Task<bool>> messageHandler,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops listening to a specific subscription.
    /// </summary>
    Task StopAsync(string subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops all active subscriptions.
    /// </summary>
    Task StopAllAsync(CancellationToken cancellationToken = default);
}
