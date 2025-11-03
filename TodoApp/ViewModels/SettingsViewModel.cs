using Microsoft.Maui;
using TodoApp.Services;

namespace TodoApp.ViewModels;
// Управляет настройками приложения, такими как тема и язык
public class SettingsViewModel : BaseViewModel
{
    private readonly ThemeService theme;

    public SettingsViewModel(ThemeService theme)
    {
        this.theme = theme;
        Title = "Seaded";

        // Инициализируем свойство из сервиса
        Teema = theme.Current;
    }

    // Свойство языка можно удалить, если не используете.
    private string keel = "et-EE";
    public string Keel
    {
        get => keel;
        set => Set(ref keel, value);
    }

    private AppTheme teema = AppTheme.Unspecified;
    public AppTheme Teema
    {
        get => teema;
        set
        {
            if (teema == value) return;
            teema = value;
            OnPropertyChanged();

            // Применяем и сохраняем через сервис
            theme.Apply(value);
            theme.Save(value);
        }
    }

    public void Save() => theme.Save(Teema);
}
