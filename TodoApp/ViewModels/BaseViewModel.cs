using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TodoApp.ViewModels;
//помогает обновлять экран, когда данные меняются
// простой базовый VM
public class BaseViewModel : INotifyPropertyChanged
{
    public string Title { get; set; } = "";

    public event PropertyChangedEventHandler? PropertyChanged;

    // Универсальный сеттер с уведомлением
    protected void Set<T>(ref T field, T value, [CallerMemberName] string? prop = null)
    {
        if (!Equals(field, value))
        {
            field = value;
            OnPropertyChanged(prop);
        }
    }

    // Явный вызов уведомления, если нужно
    protected void OnPropertyChanged([CallerMemberName] string? prop = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
}
