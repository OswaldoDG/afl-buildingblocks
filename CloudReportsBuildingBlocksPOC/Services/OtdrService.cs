namespace CloudReportsBuildingBlocksPOC.Services;

using CloudReportsBuildingBlocksPOC.Models.Otdr;
using CloudReportsBuildingBlocksPOC.Repositories;
using CloudReportsBuildingBlocksPOC.Services.Abstractions;

public partial class OtdrService : IOtdrService
{
    private readonly ILogger<OtdrService> _logger;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContextService _userContextService;

    public OtdrService(
        IUserContextService userContextService,
        ILogger<OtdrService> logger,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _userContextService = userContextService;
    }

    public async Task UpsertAtd(AtdEntry entry)
    {
        try
        {
            LogCreateEntry(entry.AtdEntryId);

            var repoAtdEntries = _unitOfWork.Repository<AtdEntry>();
            var repoJobs = _unitOfWork.Repository<AtdJob>();

            await repoAtdEntries.InsertAsync(entry);

            foreach (var job in entry.Jobs)
            {
                job.AtdEntryId = entry.AtdEntryId;
                await repoJobs.InsertAsync(job);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OtdrService-UpsertAtd Failed to create Atd entry {AtdEntryId} {Message}", entry.AtdEntryId, ex.Message);
            throw;
        }
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "OtdrService-UpsertAtd for entry {entryId}")]
    partial void LogCreateEntry(string entryId);
}
