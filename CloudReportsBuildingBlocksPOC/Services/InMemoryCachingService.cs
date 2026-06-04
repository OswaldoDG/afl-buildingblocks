namespace CloudReportsBuildingBlocksPOC.Services;

using CloudReportsBuildingBlocksPOC.Services.Abstractions;
using Microsoft.Extensions.Caching.Distributed;

public class InMemoryCachingService(IDistributedCache distributedCache) : ICachingService
{
    
}
