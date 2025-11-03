using System.Globalization;

namespace TodoApp.Services;
// Управляет локализацией приложения: сохранение и восстановление выбранного языка
public class LocalizationService
{
    const string Key = "lang";
    public void SetLanguage(string code)
    {
        var normalized = code switch { "et" => "et-EE", "en" => "en-US", _ => code };
        Preferences.Set(Key, normalized);
        try
        {
            var c = new CultureInfo(normalized);
            CultureInfo.CurrentCulture = c;
            CultureInfo.CurrentUICulture = c;
        }
        catch { /* ignore */ }
    }
    public void Restore() => SetLanguage(Preferences.Get(Key, "et-EE"));
}
