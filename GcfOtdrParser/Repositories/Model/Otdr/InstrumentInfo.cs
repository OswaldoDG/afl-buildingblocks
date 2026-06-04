#nullable enable
namespace GcfOtdrParser.Repositories.Model.Otdr;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

public class InstrumentInfo
{
    [Key]
    public int Id { get; set; }

    public DateTime? CalDate { get; set; }

    public DateTime? TimeStamp { get; set; }

    public FileModificationTypes? CreatedOrModified { get; set; }

    public string? InstrumentType { get; set; }

    public string Key { get; set; }

    public string LocatedAt { get; set; }

    public UnitModes? MainOrRemote { get; set; }

    public string Model { get; set; }

    public string SerialNumber { get; set; }

    // public List<VersionInfo> VersionInfos { get; set; } = new ();

    public static List<InstrumentInfo> FromLuna(List<AFL.Luna.Atd.Models.Atd.InstrumentInfo> infos) =>
        (infos ?? new()).Select(FromLuna).ToList();

    public static List<InstrumentInfo> FromLuna(List<AFL.Luna.Power.Models.InstrumentInfo> infos) =>
        (infos ?? new()).Select(FromLunaPower).ToList();

    public static InstrumentInfo FromLunaPower(AFL.Luna.Power.Models.InstrumentInfo info)
    {
        return new()
        {
            CalDate = info.CalDate,
            TimeStamp = info.TimeStamp,
            CreatedOrModified = PowerConverters.GetFileModificationTypes(info.CreatedOrModified),
            InstrumentType = info.InstrumentType,
            Key = info.Key,
            LocatedAt = info.LocatedAt,
            MainOrRemote = PowerConverters.GetUnitModes(info.MainOrRemote),
            Model = info.Model,
            SerialNumber = info.SerialNumber,
            // VersionInfos = VersionInfo.FromLuna(info.VersionInfos)
        };
    }

    public static InstrumentInfo FromLuna(AFL.Luna.Atd.Models.Atd.InstrumentInfo info)
    {
        return new()
        {
            CalDate = info.CalDate,
            TimeStamp = info.TimeStamp,
            CreatedOrModified = PowerConverters.GetFileModificationTypes(info.CreatedOrModified),
            InstrumentType = info.InstrumentType,
            Key = info.Key,
            LocatedAt = info.LocatedAt,
            MainOrRemote = PowerConverters.GetUnitModes(info.MainOrRemote),
            Model = info.Model,
            SerialNumber = info.SerialNumber,
            // VersionInfos = VersionInfo.FromLuna(info.VersionInfos)
        };
    }
}
