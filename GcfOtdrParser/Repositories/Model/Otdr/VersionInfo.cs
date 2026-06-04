#nullable enable
namespace GcfOtdrParser.Repositories.Model.Otdr;

using System.Collections.Generic;
using System.Linq;

public record VersionInfo(string? InstrumentType, string? VersionType, string? Version, string? SerialNumber)
{
}
