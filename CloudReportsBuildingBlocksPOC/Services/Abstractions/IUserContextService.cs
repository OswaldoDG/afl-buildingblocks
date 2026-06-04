namespace CloudReportsBuildingBlocksPOC.Services.Abstractions;

using CloudReportsBuildingBlocksPOC.Models.Context;

/// <summary>
/// Store information about the current user context making the request.
/// </summary>
public interface IUserContextService
{
    /// <summary>
    /// GEt or set the user information.
    /// </summary>
    UserContext? Context { get; set; }
}
