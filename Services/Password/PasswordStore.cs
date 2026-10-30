using System.Text.Json;
using PassChanger.Models;

namespace PassChanger.Services.Password;

public sealed class PasswordStore
{
    private const string FileName = "password_data.json";

    private readonly string _filePath;

    public PasswordStore(string moduleDirectory)
    {
        _filePath = Path.Combine(moduleDirectory, FileName);
    }

    public bool HasSavedPassword() => File.Exists(_filePath);

    public void SavePassword(string password)
    {
        File.WriteAllText(_filePath, JsonSerializer.Serialize(new PasswordData(password)));
    }

    public string LoadPassword()
    {
        if (!HasSavedPassword()) return "";

        var data = JsonSerializer.Deserialize<PasswordData>(File.ReadAllText(_filePath));
        return data?.Password ?? "";
    }
}
