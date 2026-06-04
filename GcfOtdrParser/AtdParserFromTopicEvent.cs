namespace GcfOtdrParser;

using AFL.Luna.Atd;
using AFL.Luna.Atd.Models.Atd;
using AFL.Luna.Power.Extensions;
using CloudNative.CloudEvents;
using GcfOtdrParser.Models;
using GcfOtdrParser.Services;
using Google.Cloud.Functions.Framework;
using Google.Cloud.Storage.V1;
using Google.Events.Protobuf.Cloud.PubSub.V1;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class AtdParserFromTopicEvent(ILogger<AtdParserFromTopicEvent> logger) : ICloudEventFunction<MessagePublishedData>
{

    private const string OTDRCONNSTRING = "OTDRCONNSTRING";

    public async Task HandleAsync(CloudEvent cloudEvent, MessagePublishedData data, CancellationToken cancellationToken)
    {
        try
        {
            // Access the Pub/Sub message data
            string textContent = data.Message.TextData;
            logger.LogDebug("Received message: {Message}", textContent);
            BatchCompletedEvent processData = System.Text.Json.JsonSerializer.Deserialize<BatchCompletedEvent>(textContent, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            IUnitOfWork unitOfWork = new UnitOfWork(Environment.GetEnvironmentVariable(OTDRCONNSTRING));
            OtdrService otdrService = new (logger, unitOfWork);


            var files = await BucketService.GetFilesRecursiveAsync(processData.BucketName, processData.DestinationFolder);
            foreach (var file in files)
            {
                logger.LogInformation("Processing file {FileName} in bucket {BucketName}", file, processData.BucketName);
                var reader = await ReadFile(processData.BucketName, file);
                AtdFile atdFile = AtdDecoder.DecodeATDFile(reader);
                var fibers = atdFile.GetCertFibers();
                var isValidOlts = fibers.Any(a => !a.IsValidOlts);
                if (!isValidOlts)
                {
                    logger.LogInformation("File {FileName} in bucket {BucketName} does not contain valid OLTS, skipping", file, processData.BucketName);
                }

                logger.LogInformation("File {FileName} in bucket {BucketName} valid OLTS", file, processData.BucketName);
                var atdEntry = atdFile.ToDbAdtEntry(file, Path.GetFileName(file));
                await otdrService.UpsertAtd(atdEntry);
                logger.LogInformation("File {FileName} in bucket {BucketName} processed successfully", file, processData.BucketName);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing event");
        }
    }

    private static async Task<Stream> ReadFile(string bucketName, string objectName)
    {
        var storage = await StorageClient.CreateAsync();

        var stream = new MemoryStream();

        await storage.DownloadObjectAsync(bucketName, objectName, stream);

        stream.Position = 0;

        return stream;
    }
}
