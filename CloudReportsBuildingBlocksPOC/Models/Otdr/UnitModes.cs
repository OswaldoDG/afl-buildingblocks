namespace CloudReportsBuildingBlocksPOC.Models.Otdr;

using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UnitModes
{
    Main,
    Remote,
}