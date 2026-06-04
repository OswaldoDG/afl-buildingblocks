namespace GcfOtdrParser;

using AFL.Luna.Atd;
using AFL.Luna.Atd.Models.Atd;
using AFL.Luna.Power.Extensions;
using CloudNative.CloudEvents;
using GcfOtdrParser.Services;
using Google.Cloud.Functions.Framework;
using Google.Cloud.Storage.V1;
using Google.Events.Protobuf.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class AtdParserFromBucketEvent(ILogger<AtdParserFromBucketEvent> logger) : ICloudEventFunction<StorageObjectData>
{
    public async Task HandleAsync(CloudEvent cloudEvent, StorageObjectData data, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing file {FileName} in bucket {BucketName}", data.Name, data.Bucket);
        logger.LogDebug($"  Name: {data.Name}");
        logger.LogDebug($"  Bucket: {data.Bucket}");
        logger.LogDebug($"  Size: {data.Size}");
        logger.LogDebug($"  Content type: {data.ContentType}");
        logger.LogDebug("CloudEvent information:");
        logger.LogDebug($"  ID: {cloudEvent.Id}");
        logger.LogDebug($"  Source: {cloudEvent.Source}");
        logger.LogDebug($"  Type: {cloudEvent.Type}");
        logger.LogDebug($"  Subject: {cloudEvent.Subject}");
        logger.LogDebug($"  DataSchema: {cloudEvent.DataSchema}");
        logger.LogDebug($"  DataContentType: {cloudEvent.DataContentType}");
        logger.LogDebug($"  Time: {cloudEvent.Time?.ToUniversalTime():yyyy-MM-dd'T'HH:mm:ss.fff'Z'}");
        logger.LogDebug($"  SpecVersion: {cloudEvent.SpecVersion}");

        try
        {
            var reader = await ReadFile(data.Bucket, data.Name);

            AtdFile atdFile = AtdDecoder.DecodeATDFile(reader);
            var fibers = atdFile.GetCertFibers();
            var isValidOlts = fibers.Any(a => !a.IsValidOlts);

            if (!isValidOlts)
            {
                logger.LogInformation("File {FileName} in bucket {BucketName} does not contain valid OLTS, skipping", data.Name, data.Bucket);
            }

            logger.LogInformation("File {FileName} in bucket {BucketName} valid OLTS", data.Name, data.Bucket);

            OtdrApiLCient apiLCient = new (logger);
            var token = await apiLCient.DescopeLogin();

            if (!string.IsNullOrEmpty(token))
            {
                var atdEntry = atdFile.ToAdtEntry(data.Name, Path.GetFileName(data.Name));
                await apiLCient.PostAtdEntry(atdEntry);
                logger.LogInformation("File {FileName} in bucket {BucketName} processed successfully", data.Name, data.Bucket);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing file {FileName} in bucket {BucketName}", data.Name, data.Bucket);
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
