namespace CloudReportsBuildingBlocksPOC.Services;

using CloudReportsBuildingBlocksPOC.Models.Context;
using CloudReportsBuildingBlocksPOC.Services.Abstractions;

/// <summary>
/// Stores information about the current user context making the request.
/// </summary>
public class UserContextService : IUserContextService
{
    /// <summary>
    /// Users's session data.
    /// </summary>
    public UserContext? Context { get; set; }
}
