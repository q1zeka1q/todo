// Общие настройки (язык, звук, тема)
using System.Text.Json;

namespace TodoApp.Services;

public record AppSettings(string Keel, bool HeliOn, AppTheme Teema);

public class SettingsService
{
    const string Key = "app_settings";
    public AppSettings Praegu { get; private set; } = new("et-EE", true, AppTheme.Light);

    public void Load()
    {
        var json = Preferences.Get(Key, "");
        if (!string.IsNullOrWhiteSpace(json))
            Praegu = JsonSerializer.Deserialize<AppSettings>(json)!;
    }
    public void Save() => Preferences.Set(Key, JsonSerializer.Serialize(Praegu));
    public void Update(AppSettings s) { Praegu = s; Save(); }
}
