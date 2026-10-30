using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using PassChanger.Configs;
using PassChanger.Utils;

namespace PassChanger.Services.Password;

public sealed class PasswordService : FeatureService
{
    private const string FeatureName = "Password";
    private const float OverwriteCheckDelaySeconds = 1.0f;

    private const string PrefixKey = "prefix";
    private const string NoPermissionKey = "no.permission";
    private const string PasswordChangedKey = "password.changed";
    private const string PasswordCurrentKey = "password.current";
    private const string PasswordRemovedKey = "password.removed";
    private const string PasswordNotSetKey = "password.not.set";
    private const string PasswordRemovalDisabledKey = "password.removal.disabled";

    private readonly BasePlugin _plugin;
    private readonly BaseConfig _config;
    private readonly PasswordStore _store;
    private string _originalPassword = "";

    public PasswordService(BasePlugin plugin, BaseConfig config, PasswordStore store) : base(FeatureName)
    {
        _plugin = plugin;
        _config = config;
        _store = store;
    }

    protected override void OnActivate()
    {
        _originalPassword = ServerPassword.GetCurrent();
        SaveCurrentPasswordOnFirstRun();

        _plugin.RegisterListener<Listeners.OnMapStart>(OnMapStart);
        _plugin.AddCommand(_config.Commands.ChangePassCommand, "Allows to change the server password", OnChangePasswordCommand);
        _plugin.AddCommand(_config.Commands.CurrentPassCommand, "Show current server password", OnCurrentPasswordCommand);

        // NOTE: A hot reload fires no map start, and deactivation has just restored the original password
        ApplySavedPassword();
    }

    protected override void OnDeactivate()
    {
        _plugin.RemoveListener<Listeners.OnMapStart>(OnMapStart);
        _plugin.RemoveCommand(_config.Commands.ChangePassCommand, OnChangePasswordCommand);
        _plugin.RemoveCommand(_config.Commands.CurrentPassCommand, OnCurrentPasswordCommand);

        ServerPassword.Apply(_originalPassword);
    }

    private void SaveCurrentPasswordOnFirstRun()
    {
        if (_store.HasSavedPassword()) return;

        _store.SavePassword(_originalPassword);
        Logger.LogDebug(FeatureName, "Saved the current server password for the first time");
    }

    private void OnMapStart(string mapName) => RunGuarded(ScheduleSavedPasswordApply);

    private void ScheduleSavedPasswordApply()
    {
        _plugin.AddTimer(_config.General.PasswordApplyDelay, () => RunGuarded(ApplySavedPassword), TimerFlags.STOP_ON_MAPCHANGE);
    }

    private void ApplySavedPassword()
    {
        var password = _store.LoadPassword();
        if (string.IsNullOrEmpty(password) && !_config.General.AllowPasswordRemoval) return;

        ServerPassword.Apply(password);
        Logger.LogDebug(FeatureName, "Saved password applied");

        _plugin.AddTimer(
            OverwriteCheckDelaySeconds,
            () => RunGuarded(() => ReapplyPasswordIfOverwritten(password)),
            TimerFlags.STOP_ON_MAPCHANGE);
    }

    private static void ReapplyPasswordIfOverwritten(string password)
    {
        if (ServerPassword.GetCurrent() == password) return;

        Logger.LogDebug(FeatureName, "Password was overwritten, reapplying");
        ServerPassword.Apply(password);
    }

    private void OnChangePasswordCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        RunGuarded(() => ChangePassword(player, commandInfo.GetArg(1)));
    }

    private void ChangePassword(CCSPlayerController? player, string password)
    {
        if (!CanRunCommand(player, _config.Permissions.ChangePassFlag))
        {
            ReplyToPlayer(player, NoPermissionKey);
            return;
        }

        var isRemoval = string.IsNullOrEmpty(password);
        if (isRemoval && !_config.General.AllowPasswordRemoval)
        {
            ReplyToPlayer(player, PasswordRemovalDisabledKey);
            return;
        }

        _store.SavePassword(password);
        ServerPassword.Apply(password);

        if (isRemoval)
        {
            ReplyToPlayer(player, PasswordRemovedKey);
            return;
        }

        ReplyToPlayer(player, PasswordChangedKey, password);
    }

    private void OnCurrentPasswordCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        RunGuarded(() => ShowCurrentPassword(player));
    }

    private void ShowCurrentPassword(CCSPlayerController? player)
    {
        if (!CanRunCommand(player, _config.Permissions.CurrentPassFlag))
        {
            ReplyToPlayer(player, NoPermissionKey);
            return;
        }

        var password = _store.LoadPassword();
        if (string.IsNullOrEmpty(password))
        {
            ReplyToPlayer(player, PasswordNotSetKey);
            return;
        }

        ReplyToPlayer(player, PasswordCurrentKey, password);
    }

    private static bool CanRunCommand(CCSPlayerController? player, string flag)
    {
        return player == null || AdminManager.PlayerHasPermissions(player, flag);
    }

    private void ReplyToPlayer(CCSPlayerController? player, string messageKey, params object[] arguments)
    {
        player?.PrintToChat($"{_plugin.Localizer[PrefixKey]} {_plugin.Localizer[messageKey, arguments]}");
    }
}
