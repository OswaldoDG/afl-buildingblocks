#nullable enable
namespace GcfOtdrParser.Repositories.Model.Otdr;

using System.Collections.Generic;
using System.Linq;

public record VersionInfo(string? InstrumentType, string? VersionType, string? Version, string? SerialNumber)
{
    public static List<VersionInfo> FromLuna(List<AFL.Luna.Power.Models.VersionInfo> info) =>
        (info ?? new ()).Select(FromLunaPower)
                       .ToList();
    public static List<VersionInfo> FromLuna(List<AFL.Luna.Atd.Models.Atd.VersionInfo> info) =>
        (info ?? new ()).Select(FromLuna)
                       .ToList();

    public static VersionInfo FromLuna(AFL.Luna.Atd.Models.Atd.VersionInfo info)
    {
        return new (info.InstrumentType,
                   info.VersionType?.ToString(),
                   info.Version,
                   info.SerialNumber);
    }
    public static VersionInfo FromLunaPower(AFL.Luna.Power.Models.VersionInfo info)
    {
        return new (info.InstrumentType,
                   info.VersionType?.ToString(),
                   info.Version,
                   info.SerialNumber);
    }
}
