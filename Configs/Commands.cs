using System.Text.Json.Serialization;

namespace PassChanger.Configs;

public sealed class CommandsConfig
{
    [JsonPropertyName("ChangePassCommand")]
    public string ChangePassCommand { get; set; } = "css_changepass";

    [JsonPropertyName("CurrentPassCommand")]
    public string CurrentPassCommand { get; set; } = "css_currentpass";
}
