namespace GcfOtdrParser.Repositories.Model.Otdr;

using System;

public static class PowerConverters
{

    public static AFL.Luna.Atd.Models.AtdXmlModels.DistanceUnitTypes GetUnitTypes(AFL.Luna.Core.Enums.DistanceUnitTypes unitTypes) =>
    unitTypes switch
    {
        AFL.Luna.Core.Enums.DistanceUnitTypes.Kilometers => AFL.Luna.Atd.Models.AtdXmlModels.DistanceUnitTypes.Kilometers,
        AFL.Luna.Core.Enums.DistanceUnitTypes.Miles => AFL.Luna.Atd.Models.AtdXmlModels.DistanceUnitTypes.Miles,
        AFL.Luna.Core.Enums.DistanceUnitTypes.Kilofeet => AFL.Luna.Atd.Models.AtdXmlModels.DistanceUnitTypes.Kilofeet,
        AFL.Luna.Core.Enums.DistanceUnitTypes.Meters => AFL.Luna.Atd.Models.AtdXmlModels.DistanceUnitTypes.Meters,
        AFL.Luna.Core.Enums.DistanceUnitTypes.Millimeters => AFL.Luna.Atd.Models.AtdXmlModels.DistanceUnitTypes.Millimeters,
        AFL.Luna.Core.Enums.DistanceUnitTypes.Feet => AFL.Luna.Atd.Models.AtdXmlModels.DistanceUnitTypes.Feet,
        _ => throw new NotSupportedException()
    };

    public static FileModificationTypes? GetFileModificationTypes(AFL.Luna.Power.Enums.FileModificationTypes? types) =>
        types switch
        {
            null => null,
            AFL.Luna.Power.Enums.FileModificationTypes.Created => FileModificationTypes.Created,
            AFL.Luna.Power.Enums.FileModificationTypes.Modified => FileModificationTypes.Modified,
            _ => throw new NotSupportedException()
        };

    public static UnitModes? GetUnitModes(AFL.Luna.Power.Enums.UnitModes? modes) =>
          modes switch
          {
              null => null,
              AFL.Luna.Power.Enums.UnitModes.Main => UnitModes.Main,
              AFL.Luna.Power.Enums.UnitModes.Remote => UnitModes.Remote,
              _ => throw new NotSupportedException()
          };


    public static FileModificationTypes? GetFileModificationTypes(AFL.Luna.Atd.Enums.FileModificationTypes? types) =>
    types switch
    {
        null => null,
        AFL.Luna.Atd.Enums.FileModificationTypes.Created => FileModificationTypes.Created,
        AFL.Luna.Atd.Enums.FileModificationTypes.Modified => FileModificationTypes.Modified,
        _ => throw new NotSupportedException()
    };


    public static UnitModes? GetUnitModes(AFL.Luna.Atd.Enums.UnitModes? modes) =>
    modes switch
    {
        null => null,
        AFL.Luna.Atd.Enums.UnitModes.Main => UnitModes.Main,
        AFL.Luna.Atd.Enums.UnitModes.Remote => UnitModes.Remote,
        _ => throw new NotSupportedException()
    };
}
