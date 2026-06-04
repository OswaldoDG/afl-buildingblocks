#nullable enable
namespace CloudReportsBuildingBlocksPOC.Models.Otdr;

using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FileModificationTypes
{
    Created,
    Modified,
}