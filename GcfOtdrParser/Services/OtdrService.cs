namespace GcfOtdrParser.Services;

using GcfOtdrParser.Repositories.Model.Otdr;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

public partial class OtdrService
{
    private readonly ILogger _logger;
    private readonly IUnitOfWork _unitOfWork;

    public OtdrService(
        ILogger logger,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task UpsertAtd(AtdEntry entry)
    {
        try
        {
            _logger.LogInformation("Upserting {File}", entry.FileName);

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
}
