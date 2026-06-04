#nullable enable
namespace CloudReportsBuildingBlocksPOC.Models.Otdr;

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
    
}
