namespace CloudReportsBuildingBlocksPOC.Models.Context;

public class UserContext
{
    /// <summary>
    /// Unique identifier of the user making the request, used for tracking and correlation.
    /// </summary>
    public string? UserId { get; set; }
}
