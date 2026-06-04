namespace CloudReportsBuildingBlocksPOC.Services.Abstractions;

public interface IPubSubWriterService
{
    /// <summary>
    /// Publish a single message.
    /// </summary>
    /// <typeparam name="T">base type.</typeparam>
    /// <param name="topicId">Topic id.</param>
    /// <param name="message">Content.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Message id.</returns>
    Task<string> PublishMessageAsync<T>(string topicId, T message, CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    /// Publish a message with attributes.
    /// </summary>
    /// <typeparam name="T">base type.</typeparam>
    /// <param name="topicId">Topic id.</param>
    /// <param name="message">Content.</param>
    /// <param name="attributes">Message attributes.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Message id.</returns>
    Task<string> PublishMessageAsync<T>(string topicId, T message, IDictionary<string, string>? attributes = null, CancellationToken cancellationToken = default)
        where T : class;
}
