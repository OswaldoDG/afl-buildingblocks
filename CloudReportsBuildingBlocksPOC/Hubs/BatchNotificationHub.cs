namespace CloudReportsBuildingBlocksPOC.Hubs;

using CloudReportsBuildingBlocksPOC.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

/// <summary>
/// SignalR hub for real-time batch processing notifications.
/// Clients connect with JWT authentication and receive notifications for their specific sessions.
/// </summary>
[Authorize]
public class BatchNotificationHub : Hub
{
    private readonly ISessionConnectionMappingService _sessionMapping;
    private readonly ILogger<BatchNotificationHub> _logger;

    public BatchNotificationHub(
        ISessionConnectionMappingService sessionMapping,
        ILogger<BatchNotificationHub> logger)
    {
        _sessionMapping = sessionMapping;
        _logger = logger;
    }

    /// <summary>
    /// Called when a client connects to the hub.
    /// Generates a new SessionId and returns it to the client.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? Context.User?.FindFirst("sub")?.Value;

        // Generate a new session ID
        var sessionId = Guid.NewGuid().ToString();

        // Register the mapping
        _sessionMapping.AddConnection(sessionId, connectionId);

        _logger.LogInformation(
            "SignalR connection established. ConnectionId: {ConnectionId}, UserId: {UserId}, SessionId: {SessionId}",
            connectionId, userId, sessionId);

        // Send the session ID back to the client
        await Clients.Caller.SendAsync("SessionEstablished", sessionId);

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub.
    /// Cleans up the session mapping.
    /// </summary>
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        var sessionId = _sessionMapping.GetSessionId(connectionId);
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? Context.User?.FindFirst("sub")?.Value;

        // Remove the mapping
        _sessionMapping.RemoveConnection(connectionId);

        if (exception != null)
        {
            _logger.LogWarning(exception,
                "SignalR connection closed with error. ConnectionId: {ConnectionId}, UserId: {UserId}, SessionId: {SessionId}",
                connectionId, userId, sessionId);
        }
        else
        {
            _logger.LogInformation(
                "SignalR connection closed. ConnectionId: {ConnectionId}, UserId: {UserId}, SessionId: {SessionId}",
                connectionId, userId, sessionId);
        }

        return base.OnDisconnectedAsync(exception);
    }
}
