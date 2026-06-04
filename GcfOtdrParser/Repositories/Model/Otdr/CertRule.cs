#nullable enable
namespace GcfOtdrParser.Repositories.Model.Otdr;

using AFL.Luna.Atd.Models.Atd;
using AFL.Luna.Atd.Models.AtdXmlModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using LunaModels = AFL.Luna.Atd.Models;

public class CertRule
{
    [Key]
    public int Id { get; set; }
    public double MaxLength { get; set; }

    public DistanceUnitTypes MaxLengthUnit { get; set; }

    public string Name { get; set; }

    public float? LossPerConnector { get; set; }

    public float? LossPer2ndConnector { get; set; }

    public float? LossPer3rdConnector { get; set; }

    public float? LossPer4thConnector { get; set; }

    public float? LossPerSplice { get; set; }

    public double? LossPerTestCordToTestCord { get; set; }

    public double? LossPerTestCordToFut { get; set; }

    public TestPortTypes? FiberType { get; set; }

    public string Description { get; set; }

    public RuleTypes? LossType { get; set; }

    public StandardsTypes? StdType { get; set; }

    public RuleLossTypes? LossThreshType { get; set; }

    public PowerUnits? PowerUnits { get; set; }

    public CategoryTypes? CatType { get; set; }

    public FiberTypeActual? ActualFiberType { get; set; }

    public UseTypes? UseType { get; set; }

    public CalculationTypes? CalculationToUse { get; set; }

    public LengthThreshTypes? LengthThreshType { get; set; }

    public List<LossThreshold> LossThresholds { get; set; } = new ();

    public static List<CertRule> FromLuna(List<LunaModels.Atd.CertRule> rule) =>
        (rule ?? new ()).Select(FromLuna)
                       .ToList();

    public static CertRule FromLuna(LunaModels.Atd.CertRule rule)
    {
        return new ()
        {
            LossPer2ndConnector = (float?)rule.LossPer2ndConnector,
            LossPer3rdConnector = (float?)rule.LossPer3rdConnector,
            LossPer4thConnector = (float?)rule.LossPer4thConnector,
            LossPerConnector = (float?)rule.LossPerConnector,
            LossPerSplice = (float?)rule.LossPerSplice,
            LossPerTestCordToFut = rule.LossPerTestCordToFut,
            LossPerTestCordToTestCord = rule.LossPerTestCordToTestCord,
            MaxLengthUnit = PowerConverters.GetUnitTypes(rule.MaxLengthUnit),
            Name = rule.Name,
            MaxLength = rule.MaxLength,
        };
    }
}
