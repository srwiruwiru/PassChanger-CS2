using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace PassChanger.Configs;

public sealed class BaseConfig : BasePluginConfig
{
    [JsonPropertyName("Commands")]
    public CommandsConfig Commands { get; set; } = new();

    [JsonPropertyName("Permissions")]
    public PermissionsConfig Permissions { get; set; } = new();

    [JsonPropertyName("General")]
    public GeneralConfig General { get; set; } = new();

    [JsonPropertyName("ConfigVersion")]
    public override int Version { get; set; } = 1;
}
