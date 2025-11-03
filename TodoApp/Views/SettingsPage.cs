using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using TodoApp.ViewModels;

namespace TodoApp.Views;
// Страница настроек приложения: тема и внешний вид
public class SettingsPage : ContentPage
{
    private readonly Picker themePicker;

    public SettingsPage()
    {
        Title = "Seaded";
        BindingContext = App.Current?.Handler?.MauiContext?.Services.GetService<SettingsViewModel>();

        // Шапка (Border вместо Grid)
        var header = new Border
        {
            Padding = new Thickness(18, 22),
            Background = new LinearGradientBrush(
                new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb("#4F46E5"), 0f),
                    new GradientStop(Color.FromArgb("#22D3EE"), 1f)
                },
                new Point(0, 0), new Point(1, 1)
            ),
            StrokeShape = new RoundRectangle { CornerRadius = 18 },
        };

        header.Content = new VerticalStackLayout
        {
            Spacing = 2,
            Children =
            {
                new Label
                {
                    Text = "Rakenduse seaded",
                    FontSize = 22,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White
                },
                new Label
                {
                    Text = "Teema ja välimus",
                    FontSize = 14,
                    TextColor = Color.FromArgb("#E5E7EB")
                }
            }
        };

        // Карточка темы
        themePicker = new Picker { Title = "Vali teema" };
        themePicker.Items.Add("Süsteemi");
        themePicker.Items.Add("Hele");
        themePicker.Items.Add("Tume");
        themePicker.SelectedIndexChanged += OnThemeChanged;

        var themeCard = new Border
        {
            Style = (Style)Application.Current!.Resources["Card"],
            Content = new VerticalStackLayout
            {
                Spacing = 6,
                Children =
                {
                    new Label { Text = "Teema:", FontSize = 18, FontAttributes = FontAttributes.Bold },
                    new Label { Text = "Vali rakenduse teema", FontSize = 13, TextColor = Colors.Gray },
                    themePicker
                }
            }
        };

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 16,
                Spacing = 16,
                Children = { header, themeCard }
            }
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is SettingsViewModel vm)
        {
            themePicker.SelectedIndex = vm.Teema switch
            {
                AppTheme.Light => 1,
                AppTheme.Dark => 2,
                _ => 0
            };
        }
    }

    private void OnThemeChanged(object? s, EventArgs e)
    {
        if (BindingContext is not SettingsViewModel vm) return;

        vm.Teema = themePicker.SelectedIndex switch
        {
            1 => AppTheme.Light,
            2 => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };

        vm.Save();
    }
}
