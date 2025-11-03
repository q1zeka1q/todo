using Microsoft.Maui;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Dispatching;

namespace TodoApp.Services;
// Управляет темой приложения: сохранение, загрузка и применение
public class ThemeService
{
    private const string PrefKey = "app_theme_v1";

    public AppTheme Current { get; private set; } = AppTheme.Unspecified;

    public ThemeService()
    {
        // Только читаем сохранённое значение, без немедленного применения
        Load();
    }

    /// Применить тему. Безопасно, даже если Application.Current ещё null
    public void Apply(AppTheme theme)
    {
        Current = theme;

        // Пробуем сразу
        if (Application.Current is not null)
        {
            Application.Current.UserAppTheme = theme;
            return;
        }

        // Если ещё рано — отложим применение на главный поток,
        // когда Application.Current уже будет создан
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (Application.Current is not null)
                Application.Current.UserAppTheme = theme;
        });
    }

    public void Save(AppTheme theme)
    {
        Preferences.Set(PrefKey, (int)theme);
        Current = theme;
    }

    /// Загружаем сохранённую тему (НЕ применяем сразу)
    public void Load()
    {
        Current = (AppTheme)Preferences.Get(PrefKey, (int)AppTheme.Unspecified);
    }

    /// Совместимость с твоим кодом: Restore = Load + попытка применить
    public void Restore()
    {
        Load();
        Apply(Current);
    }
}
