using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using TodoApp.Services;
using TodoApp.ViewModels;
using TodoApp.Views;

namespace TodoApp;
// Настройка и создание основного приложения MAUI
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // ✨ Сервисы
        builder.Services.AddSingleton<ThemeService>();
        builder.Services.AddSingleton<DatabaseService>();

        // ✨ ViewModels
        builder.Services.AddSingleton<TasksViewModel>();
        builder.Services.AddSingleton<SettingsViewModel>();

        // ✨ Pages
        builder.Services.AddTransient<TasksPage>();

        return builder.Build();
    }
}
