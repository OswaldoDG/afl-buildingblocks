#nullable enable
namespace GcfOtdrParser.Repositories.Model.Otdr;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LunaModels = AFL.Luna.Atd.Models;

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

    public static AtdJob FromLuna(LunaModels.Atd.AtdJob job)
    {
        return new AtdJob
        {
            ContractorName = job.Contractor?.Name,
            CustomerName = job.Customer?.Name,
            JobName = job.Name,
            Operator = job.Contractor?.Operator,
            Operator2 = job.Contractor?.Operator2,
            SerializeID = job.SerializeID
        };
    }
}
