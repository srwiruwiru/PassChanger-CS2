using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Timers;
using System.Text.Json;

using PassChanger.Utils;
using PassChanger.Configs;

namespace PassChanger;

public class PasswordData
{
	public string Password { get; set; } = "";
}

[MinimumApiVersion(374)]
public class PassChanger : BasePlugin, IPluginConfig<BaseConfig>
{
	public override string ModuleName => "PassChanger";
	public override string ModuleVersion => "1.0.3";
	public override string ModuleAuthor => "luca.uy";
	public override string ModuleDescription => "Set a password for the server from a command.";

	private string _configPath => Path.Combine(ModuleDirectory, "password_data.json");

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

		if (!File.Exists(_configPath))
		{
			var currentPassword = ConVar.Find("sv_password")?.StringValue ?? "";
			SavePassword(currentPassword);

			if (!string.IsNullOrEmpty(currentPassword))
			{
				Utils.Logger.LogDebug("", "Created password_data.json with current server password");
			}
			else
			{
				Utils.Logger.LogDebug("", "Created empty password_data.json (no server password set)");
			}
		}

		RegisterListener<Listeners.OnMapStart>(OnMapStart);
		RegisterCommandHandlers();
	}

	private void RegisterCommandHandlers()
	{
		AddCommand(Config.ChangePassCommand, "Allows to change the server password", OnChangePasswordCommand);
		AddCommand(Config.CurrentPassCommand, "Show current server password", OnCurrentPasswordCommand);
	}

	private void OnMapStart(string mapName)
	{
		float delaySeconds = Config?.PasswordApplyDelay ?? 3.0f;

		AddTimer(delaySeconds, () =>
		{
			var password = LoadPassword();
			if (!string.IsNullOrEmpty(password) || (Config != null && Config.AllowPasswordRemoval))
			{
				Server.ExecuteCommand(string.IsNullOrEmpty(password) ? "sv_password \"\"" : $"sv_password \"{password}\"");
				Utils.Logger.LogDebug("", $"Password applied after {delaySeconds} seconds delay");

				AddTimer(1.0f, () =>
				{
					var currentPass = ConVar.Find("sv_password")?.StringValue;
					if (currentPass != password)
					{
						Utils.Logger.LogDebug("", "Warning: Password was overwritten, reapplying...");
						Server.ExecuteCommand(string.IsNullOrEmpty(password) ? "sv_password \"\"" : $"sv_password \"{password}\"");
					}
				});
			}
		}, TimerFlags.STOP_ON_MAPCHANGE);
	}

	private void OnChangePasswordCommand(CCSPlayerController? player, CommandInfo commandInfo)
	{
		if (player != null && Config.ChangePassFlag != null && !AdminManager.PlayerHasPermissions(player, Config.ChangePassFlag))
		{
			string noPermissionResponse = $"{Localizer["prefix"]} {Localizer["no.permission"]}";
			player.PrintToChat(noPermissionResponse);
			return;
		}

		var password = commandInfo.GetArg(1);

		if (string.IsNullOrEmpty(password) && !Config.AllowPasswordRemoval)
		{
			string removalNotAllowed = $"{Localizer["prefix"]} {Localizer["password.removal.disabled"]}";
			if (player != null)
			{
				player.PrintToChat(removalNotAllowed);
			}
			return;
		}

		SavePassword(password);

		if (string.IsNullOrEmpty(password))
		{
			Server.ExecuteCommand("sv_password \"\"");
		}
		else
		{
			Server.ExecuteCommand($"sv_password \"{password}\"");
		}

		string passwordResponse = string.IsNullOrEmpty(password)
			? $"{Localizer["prefix"]} {Localizer["password.removed"]}"
			: $"{Localizer["prefix"]} {Localizer["password.changed", password]}";

		if (player != null)
		{
			player.PrintToChat(passwordResponse);
		}
	}

	private void OnCurrentPasswordCommand(CCSPlayerController? player, CommandInfo commandInfo)
	{
		if (player != null && Config.CurrentPassFlag != null && !AdminManager.PlayerHasPermissions(player, Config.CurrentPassFlag))
		{
			string noPermissionResponse = $"{Localizer["prefix"]} {Localizer["no.permission"]}";
			player.PrintToChat(noPermissionResponse);
			return;
		}

		var password = LoadPassword();
		string passwordResponse = string.IsNullOrEmpty(password)
			? $"{Localizer["prefix"]} {Localizer["password.not.set"]}"
			: $"{Localizer["prefix"]} {Localizer["password.current", password]}";

		if (player != null)
		{
			player.PrintToChat(passwordResponse);
		}
	}

	private void SavePassword(string password)
	{
		try
		{
			var data = new PasswordData { Password = password };
			var json = JsonSerializer.Serialize(data);
			File.WriteAllText(_configPath, json);
		}
		catch (Exception ex)
		{
			Utils.Logger.LogDebug("", $"Error saving password: {ex.Message}");
		}
	}

	private string LoadPassword()
	{
		try
		{
			if (File.Exists(_configPath))
			{
				var json = File.ReadAllText(_configPath);
				var data = JsonSerializer.Deserialize<PasswordData>(json);
				return data?.Password ?? "";
			}
			return "";
		}
		catch (Exception ex)
		{
			Utils.Logger.LogDebug("", $"Error loading password: {ex.Message}");
			return "";
		}
	}

}