#nullable enable
namespace GcfOtdrParser.Repositories.Model.Otdr;

using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FileModificationTypes
{
    Created,
    Modified,
}