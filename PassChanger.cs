using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;

using PassChanger.Configs;
using PassChanger.Services.Password;

namespace PassChanger;

[MinimumApiVersion(374)]
public class PassChanger : BasePlugin, IPluginConfig<BaseConfig>
{
	public override string ModuleName => "PassChanger";
	public override string ModuleVersion => "1.0.3";
	public override string ModuleAuthor => "luca.uy";
	public override string ModuleDescription => "Set a password for the server from a command.";

	private PasswordService? _passwordService;

	public required BaseConfig Config { get; set; } = new();
	public void OnConfigParsed(BaseConfig config)
	{
		if (config.Version < Config.Version)
		{
			Utils.Logger.LogWarning("Config", $"Configuration version mismatch (Expected: {Config.Version} | Current: {config.Version})");
		}

		Config = config;
		Utils.Logger.Config = config;
	}

	public override void Load(bool hotReload)
	{
		if (hotReload)
		{
			Utils.Logger.LogInfo("Chore", "Reloading plugin...");
		}

		_passwordService = new PasswordService(this, Config, new PasswordStore(ModuleDirectory));
		_passwordService.Activate();
	}

	public override void Unload(bool hotReload)
	{
		_passwordService?.Deactivate();
	}
}
