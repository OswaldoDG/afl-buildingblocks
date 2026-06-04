namespace CloudReportsBuildingBlocksPOC.Services;

using CloudReportsBuildingBlocksPOC.Services.Abstractions;
using System.Collections.Concurrent;

/// <summary>
/// Thread-safe service for managing mappings between session IDs and SignalR connection IDs.
/// Uses in-memory storage with ConcurrentDictionary for thread safety.
/// </summary>
public class SessionConnectionMappingService : ISessionConnectionMappingService
{
    private readonly ConcurrentDictionary<string, HashSet<string>> _sessionToConnections = new();
    private readonly ConcurrentDictionary<string, string> _connectionToSession = new();
    private readonly object _lock = new();

    /// <inheritdoc/>
    public void AddConnection(string sessionId, string connectionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId, nameof(sessionId));
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId, nameof(connectionId));

        lock (_lock)
        {
            // Add to sessionId -> connectionIds mapping
            _sessionToConnections.AddOrUpdate(
                sessionId,
                new HashSet<string> { connectionId },
                (key, existingSet) =>
                {
                    existingSet.Add(connectionId);
                    return existingSet;
                });

            // Add to connectionId -> sessionId mapping
            _connectionToSession[connectionId] = sessionId;
        }
    }

    /// <inheritdoc/>
    public void RemoveConnection(string connectionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId, nameof(connectionId));

        lock (_lock)
        {
            // Find and remove from connectionId -> sessionId mapping
            if (_connectionToSession.TryRemove(connectionId, out var sessionId))
            {
                // Remove from sessionId -> connectionIds mapping
                if (_sessionToConnections.TryGetValue(sessionId, out var connections))
                {
                    connections.Remove(connectionId);

                    // Remove session entry if no more connections
                    if (connections.Count == 0)
                    {
                        _sessionToConnections.TryRemove(sessionId, out _);
                    }
                }
            }
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> GetConnections(string sessionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId, nameof(sessionId));

        if (_sessionToConnections.TryGetValue(sessionId, out var connections))
        {
            lock (_lock)
            {
                return connections.ToList();
            }
        }

        return Array.Empty<string>();
    }

    /// <inheritdoc/>
    public string? GetSessionId(string connectionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId, nameof(connectionId));

        _connectionToSession.TryGetValue(connectionId, out var sessionId);
        return sessionId;
    }
}
