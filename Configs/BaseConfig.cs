using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace PassChanger.Configs;

public sealed class BaseConfig : BasePluginConfig
{
    [JsonPropertyName("General")]
    public GeneralConfig General { get; set; } = new();

    [JsonPropertyName("Commands")]
    public CommandsConfig Commands { get; set; } = new();

    [JsonPropertyName("Permissions")]
    public PermissionsConfig Permissions { get; set; } = new();

    [JsonPropertyName("EnableDebug")]
    public bool EnableDebug { get; set; } = false;

    [JsonPropertyName("ConfigVersion")]
    public override int Version { get; set; } = 1;
}