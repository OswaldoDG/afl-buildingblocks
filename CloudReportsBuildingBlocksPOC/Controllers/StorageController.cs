namespace CloudReportsBuildingBlocksPOC.Controllers;

using CloudReportsBuildingBlocksPOC.Services.Abstractions;
using CloudReportsBuildingBlocksPOC.Models.FileSystem;

/// <summary>
/// Controller for managing storage operations.
/// </summary>
/// <param name="logger">The logger instance.</param>
/// <param name="fileSystemProvider">The file system provider.</param>
/// <param name="configuration">The configuration instance.</param>
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[Consumes("application/json")]
[ApiController]
public partial class StorageController(ILogger<StorageController> logger, IFileSystemProvider fileSystemProvider, IConfiguration configuration) : ControllerBase
{
    private readonly string _tempDirectory = configuration.GetValue<string>("TempDirectory") ?? "temp";

    [HttpPost("content")]
    public async Task<List<BlobEntry>> GetContentEntries([FromBody] BlobQueryRequest request)
    {
        LogGetContentEntries(request.Prefix);
        List<BlobEntry> entries = await fileSystemProvider.GetContent(request.Prefix, request.Recursive, request.Flattened, request.IncludeDownloadUrl);
        return entries;
    }

    [HttpPost("content/zip")]
    public async Task<ActionResult> ZipContentEntries([FromBody] BlobQueryRequest request)
    {
        LogZipContentEntries(request.Prefix);
        string tempZipPath = System.IO.Path.Combine(_tempDirectory, $"{Guid.NewGuid()}.zip");
        string zipFile = await fileSystemProvider.ZipContent(request.Prefix, tempZipPath, request.Recursive);
        if (zipFile != null)
        {
            var stream = new FileStream(zipFile, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
            string fileName = $"{request.Prefix.Replace('/', '_')}download_{DateTime.UtcNow:yyyyMMddHHmmss}.zip";

            return File(stream, "application/zip", fileName);
        }

        return NotFound("Could not create zip file");
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "StorageController-GetContentEntries called with prefix: {prefix}")]
    partial void LogGetContentEntries(string prefix);

    [LoggerMessage(Level = LogLevel.Debug, Message = "StorageController-ZipContentEntries called with prefix: {prefix}")]
    partial void LogZipContentEntries(string prefix);
}
