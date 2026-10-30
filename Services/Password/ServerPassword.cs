using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Cvars;

namespace PassChanger.Services.Password;

public static class ServerPassword
{
    private const string ConVarName = "sv_password";

    public static string GetCurrent() => ConVar.Find(ConVarName)?.StringValue ?? "";

    public static void Apply(string password)
    {
        Server.ExecuteCommand($"{ConVarName} \"{password}\"");
    }
}
