using System.Text.Json.Serialization;

namespace PassChanger.Configs;

public sealed class GeneralConfig
{
    [JsonPropertyName("AllowPasswordRemoval")]
    public bool AllowPasswordRemoval { get; set; } = true;

    [JsonPropertyName("PasswordApplyDelay")]
    public float PasswordApplyDelay { get; set; } = 1.0f;
}
