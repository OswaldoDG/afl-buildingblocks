namespace CloudReportsBuildingBlocksPOC.Services;

using CloudReportsBuildingBlocksPOC.Configuration;
using CloudReportsBuildingBlocksPOC.Hubs;
using CloudReportsBuildingBlocksPOC.Models.Batches;
using CloudReportsBuildingBlocksPOC.Models.Events;
using CloudReportsBuildingBlocksPOC.Repositories;
using CloudReportsBuildingBlocksPOC.Services.Abstractions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Kiota.Abstractions;
using System.Text.RegularExpressions;

public class FileBatchService : IFileBatchService
{
    private readonly ILogger<FileBatchService> _logger;
    private readonly IFileSystemProvider _fileSystemProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContextService _userContextService;
    private readonly IPubSubWriterService _pubSubWriter;
    private readonly PubSubOptions _pubSubOptions;
    private readonly IConfiguration _configuration;
    private readonly IHubContext<BatchNotificationHub> _hubContext;
    private readonly ISessionConnectionMappingService _sessionMapping;

    public FileBatchService(
        IOptions<PubSubOptions> pubSubOptions,
        IPubSubWriterService pubSubWriter,
        IUserContextService userContextService,
        ILogger<FileBatchService> logger,
        IFileSystemProvider fileSystemProvider,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        IHubContext<BatchNotificationHub> hubContext,
        ISessionConnectionMappingService sessionMapping)
    {
        _logger = logger;
        _fileSystemProvider = fileSystemProvider;
        _unitOfWork = unitOfWork;
        _userContextService = userContextService;
        _pubSubWriter = pubSubWriter;
        _pubSubOptions = pubSubOptions.Value;
        _configuration = configuration;
        _hubContext = hubContext;
        _sessionMapping = sessionMapping;
    }

    public async Task<bool> Complete(BatchCompleteRequestDto completeRequest)
    {
        var repoBatch = _unitOfWork.Repository<BatchEntity>();
        var batch = await repoBatch.GetByIdAsync(completeRequest.Id);

        if (batch == null)
        {
            _logger.LogError("FileBatchService-Complete Batch with id {Id} not found", completeRequest.Id);
            return false;
        }

        var repoBatchItem = _unitOfWork.Repository<BatchItemEntity>();
        var items = await GetBatchItems(completeRequest.Id);
        var okIds = completeRequest.Items.Where(i => i.Errored == null || i.Errored == false).Select(i => i.Id).ToList();
        var nookIds = completeRequest.Items.Where(i => i.Errored == true).Select(i => i.Id).ToList();

        // Transaction is needed too to allow parallel operations.
        await _unitOfWork.BeginTransactionAsync();
        if (okIds.Count != 0)
        {
            var update = items.Where(i => okIds.Contains(i.Id));
            update.ToList().ForEach(i => i.Status = FileStatus.Uploaded);
            foreach (var item in update)
            {
                await repoBatchItem.UpdateAsync(item, _unitOfWork.Transaction);
            }
        }

        if (nookIds.Count != 0)
        {
            batch.ErrorMessage = $"Upload failed for {nookIds.Count} items";
            var update = items.Where(i => nookIds.Contains(i.Id));
            foreach (var item in update)
            {
                var error = completeRequest.Items.FirstOrDefault(i => i.Id == item.Id);
                if (error != null)
                {
                    item.Status = FileStatus.Failed;
                    item.ErrorMessage = error!.ErrorMessage;
                }
            }

            foreach (var item in update)
            {
                await repoBatchItem.UpdateAsync(item, _unitOfWork.Transaction);
            }
        }

        batch.BatchStatus = nookIds.Count == 0 ? BatchStatus.Completed : BatchStatus.Failed;
        batch.ClosingDate = DateTime.UtcNow;

        await repoBatch.UpdateAsync(batch, _unitOfWork.Transaction);
        await _unitOfWork.CommitAsync();

        if (batch.BatchStatus == BatchStatus.Completed)
        {
            BatchCompletedEvent batchCompleted = new ()
            {
                Id = batch.Id,
                BucketName = _configuration["GcpStorage:BucketName"],
                Name = batch.Name,
                DestinationFolder = batch.DestinationFolder,
                SessionId = batch.SessionId,
                Items = [.. items.Select(i => new BatchCompletedItem
                {
                    FileName = i.FileName,
                    Id = i.Id,
                })],
            };

            await _pubSubWriter.PublishMessageAsync(_pubSubOptions.FileEventsTopic, batchCompleted, CancellationToken.None);

            if (!string.IsNullOrEmpty(batch.SessionId))
            {
                var connections = _sessionMapping.GetConnections(batch.SessionId);
                foreach (var connection in connections)
                {
                    await _hubContext.Clients.Client(connection).SendAsync("BatchCompleted", batchCompleted);
                }
            }
        }

        return true;
    }

    public async Task<IEnumerable<BatchItemEntity>> GetBatchItems(long batchId)
    {
        var repoBatchItem = _unitOfWork.Repository<BatchItemEntity>();
        return await repoBatchItem.QueryAsync("select *  from batchitems where batchid = @BatchId", new { BatchId = batchId });
    }

    public async Task<BatchResponseDto?> Create(BatchRequestDto batchRequest)
    {
        _logger.LogDebug("FileBatchService-Create Creating batch for {ItemCount} items with name: {BatchName}", batchRequest.Items.Count, batchRequest.Name);

        try
        {
            var repoBatch = _unitOfWork.Repository<BatchEntity>();
            var repoBatchItem = _unitOfWork.Repository<BatchItemEntity>();
            var batchEntity = new BatchEntity
            {
                Name = batchRequest.Name,
                DestinationFolder = batchRequest.DestinationFolder,
                BatchStatus = BatchStatus.Pending,
                CreationDate = DateTime.UtcNow,
                UserId = _userContextService.Context!.UserId!,
                SessionId = batchRequest.SessionId,
            };

            var batchId = await repoBatch.InsertAsync(batchEntity);

            var batchResponse = new BatchResponseDto
            {
                Id = batchId,
                Name = batchEntity.Name,
                DestinationFolder = batchEntity.DestinationFolder,
                BatchStatus = batchEntity.BatchStatus,
                CreationDate = batchEntity.CreationDate,
            };

            var items = batchRequest.Items.Select(i => new BatchItemEntity
            {
                FileName = i.FileName,
                Size = i.Size,
                Status = FileStatus.Uploading,
                BatchId = batchId,
                ItemId = i.ItemId,
            }).ToList();

            await _unitOfWork.BeginTransactionAsync();
            await repoBatchItem.BulkInsertAsync(items);
            await _unitOfWork.CommitAsync();

            var batchItems = await GetBatchItems(batchId);
            int signedUrlMinutes = _configuration.GetValue<int>("GcpStorage:SignedUrlExpirationMinutes", 60);

            List<string> files = batchItems.Select(f => $"{batchEntity.DestinationFolder}/{f.FileName}").ToList();
            List<string> uploadUrls = await _fileSystemProvider.CreateUploadUrl(files, signedUrlMinutes);

            int index = 0;
            foreach (var item in batchItems)
            {
                var itemResponse = new BatchItemResponseDto
                {
                    Id = item.Id,
                    ItemId = item.ItemId,
                    UploadUrl = uploadUrls[index],
                };
                index++;
                batchResponse.Items.Add(itemResponse);
            }

            return batchResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FileBatchService-Create Failed to create batch {BatchName} {Message}", batchRequest.Name, ex.Message);
            throw;
        }
    }

    public async Task CompleteSession(string sessionId)
    {
        if (!string.IsNullOrEmpty(sessionId))
        {
            var connections = _sessionMapping.GetConnections(sessionId);
            foreach (var connection in connections)
            {
                await _hubContext.Clients.Client(connection).SendAsync("BatchCompleted", $"Batch completed for session {sessionId} {DateTime.Now.Ticks}");
            }
        }
    }
}
