#nullable enable
namespace CloudReportsBuildingBlocksPOC.Models.Otdr;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("atdjob")]
public class AtdJob
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long? Id { get; set; }

    [Column]
    public string? AtdEntryId { get; set; }

    [Column]
    public string? ContractorName { get; set; }

    [Column]
    public string? Operator { get; set; }

    [Column]
    public string? Operator2 { get; set; }

    [Column]
    public string JobName { get; set; } = string.Empty;

    [Column]
    public string? CustomerName { get; set; }

    [Column]
    public int? SerializeID { get; set; }
}
