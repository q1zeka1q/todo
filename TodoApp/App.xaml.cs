using Microsoft.Maui;
using TodoApp.Services;

namespace TodoApp;
//запускает приложение включает тему и язык при старте
public partial class App : Application
{
    private readonly ThemeService _theme;

    public App(ThemeService theme)
    {
        InitializeComponent();
        _theme = theme;

        // теперь Restore() внутри сам аккуратно применит тему,
        // даже если Application.Current ещё не готов
        _theme.Restore();
    }

    protected override Window CreateWindow(IActivationState? activationState)
        => new(new AppShell());
}
