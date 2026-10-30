using System.Text.Json.Serialization;

namespace PassChanger.Configs;

public sealed class CommandsConfig
{
    [JsonPropertyName("ChangePassCommands")]
    public List<string> ChangePassCommands { get; set; } = ["css_changepass"];

    [JsonPropertyName("CurrentPassCommands")]
    public List<string> CurrentPassCommands { get; set; } = ["css_currentpass", "css_password"];
}