using PassChanger.Configs;

namespace PassChanger.Utils;

public static class Logger
{
    private const string PluginName = "PassChanger";
    public static BaseConfig? Config { get; set; }

    public static void LogInfo(string category, string message) => WriteLine(ConsoleColor.Green, category, message);

    public static void LogWarning(string category, string message) => WriteLine(ConsoleColor.Yellow, category, message);

    public static void LogError(string category, string message) => WriteLine(ConsoleColor.Red, category, message);

    public static void LogDebug(string category, string message)
    {
        if (Config?.EnableDebug != true) return;
        WriteLine(ConsoleColor.Cyan, category, message);
    }

    private static void WriteLine(ConsoleColor color, string category, string message)
    {
        Console.ForegroundColor = color;
        Console.WriteLine($"[{PluginName} - {category}] {message}");
        Console.ResetColor();
    }
}
