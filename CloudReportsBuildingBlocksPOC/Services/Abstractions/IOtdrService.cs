using CloudReportsBuildingBlocksPOC.Models.Otdr;

namespace CloudReportsBuildingBlocksPOC.Services.Abstractions;

public interface IOtdrService
{
    Task UpsertAtd(AtdEntry entry);
}
