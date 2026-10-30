using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace PassChanger.Configs;

public sealed class BaseConfig : BasePluginConfig
{
    [JsonPropertyName("ChangePassCommand")]
    public string ChangePassCommand { get; set; } = "css_changepass";

    [JsonPropertyName("ChangePassFlag")]
    public string ChangePassFlag { get; set; } = "@css/generic";

    [JsonPropertyName("CurrentPassCommand")]
    public string CurrentPassCommand { get; set; } = "css_currentpass";

    [JsonPropertyName("CurrentPassFlag")]
    public string? CurrentPassFlag { get; set; } = "@css/generic";

    [JsonPropertyName("AllowPasswordRemoval")]
    public bool AllowPasswordRemoval { get; set; } = true;

    [JsonPropertyName("PasswordApplyDelay")]
    public float PasswordApplyDelay { get; set; } = 2.0f;

    [JsonPropertyName("EnableDebug")]
    public bool EnableDebug { get; set; } = false;

    [JsonPropertyName("ConfigVersion")]
    public override int Version { get; set; } = 1;
}