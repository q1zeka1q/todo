// Локализация в XAML: Text="{svc:TExtension Key=Lisa}"
using Microsoft.Maui.Controls;
using TodoApp.Resources.Localization;

namespace TodoApp.Services;

[ContentProperty(nameof(Key))]
public class TExtension : IMarkupExtension
{
    public string? Key { get; set; }
    public object ProvideValue(IServiceProvider sp)
    {
        if (string.IsNullOrWhiteSpace(Key)) return string.Empty;
        return AppResources.ResourceManager.GetString(Key, AppResources.Culture) ?? Key;
    }
}
