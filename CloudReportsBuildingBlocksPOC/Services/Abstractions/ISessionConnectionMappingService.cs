namespace CloudReportsBuildingBlocksPOC.Services.Abstractions;

/// <summary>
/// Service for managing mappings between session IDs and SignalR connection IDs.
/// </summary>
public interface ISessionConnectionMappingService
{
    /// <summary>
    /// Adds a connection to a session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="connectionId">The SignalR connection identifier.</param>
    void AddConnection(string sessionId, string connectionId);

    /// <summary>
    /// Removes a connection and its associated session mapping.
    /// </summary>
    /// <param name="connectionId">The SignalR connection identifier to remove.</param>
    void RemoveConnection(string connectionId);

    /// <summary>
    /// Gets all connection IDs associated with a session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <returns>A collection of connection IDs, or empty if session has no active connections.</returns>
    IReadOnlyList<string> GetConnections(string sessionId);

    /// <summary>
    /// Gets the session ID associated with a connection.
    /// </summary>
    /// <param name="connectionId">The SignalR connection identifier.</param>
    /// <returns>The session ID, or null if the connection is not found.</returns>
    string? GetSessionId(string connectionId);
}
