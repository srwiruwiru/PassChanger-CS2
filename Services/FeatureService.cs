using CounterStrikeSharp.API;
using PassChanger.Utils;

namespace PassChanger.Services;

public abstract class FeatureService
{
    private readonly string _featureName;
    private bool _hasFailed;

    protected FeatureService(string featureName)
    {
        _featureName = featureName;
    }

    public bool IsActive { get; private set; }

    public void Activate()
    {
        if (IsActive) return;

        IsActive = true;
        RunGuarded(OnActivate);
    }

    public void Deactivate()
    {
        if (!IsActive) return;

        IsActive = false;
        try
        {
            OnDeactivate();
        }
        catch (Exception exception)
        {
            Logger.LogError(_featureName, $"Failed to deactivate: {exception.Message}");
        }
    }

    protected abstract void OnActivate();

    protected abstract void OnDeactivate();

    protected void RunGuarded(Action action)
    {
        if (!IsActive || _hasFailed) return;

        try
        {
            action();
        }
        catch (Exception exception)
        {
            _hasFailed = true;
            Logger.LogError(_featureName, $"Disabled after an error: {exception.Message}");
            // NOTE: Deferred because the failure may come from inside a hook that deactivation unregisters
            Server.NextFrame(Deactivate);
        }
    }
}
