using System.Text.Json.Serialization;

namespace PassChanger.Configs;

public sealed class PermissionsConfig
{
    [JsonPropertyName("ChangePassFlag")]
    public string ChangePassFlag { get; set; } = "@css/generic";

    [JsonPropertyName("CurrentPassFlag")]
    public string CurrentPassFlag { get; set; } = "@css/generic";
}
