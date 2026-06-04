#nullable enable
namespace CloudReportsBuildingBlocksPOC.Models.Otdr;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("atdentry")]
public sealed class AtdEntry
{
    [Key]
    public string? AtdEntryId { get; set; } = Guid.NewGuid().ToString();

    [Column]
    public string FilePath { get; set; }

    [Column]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    [Column]
    public string? FileName { get; set; }

    [Column]
    public string? OtdrSharedObjectsPath { get; set; }

    [Column]
    public string? Warnings { get; set; }

    [Column]
    public string? ErrorMessage { get; set; }

    [Column]
    public int ErrorCode { get; set; }

    [Column]
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    [Column]
    public string? Version { get; set; }

    public List<AtdJob> Jobs { get; set; } = [];

    public List<CertRule> Rules { get; set; } = [];

    public List<InstrumentInfo> Instruments { get; set; } = [];

    /// <summary>
    /// Returns whether all the entries in this AtdFile are valid Olts or not.
    /// </summary>
    public bool AreAllValidOlts { get; init; }
}
