// Короткий звук клика
using Plugin.Maui.Audio;

namespace TodoApp.Services;

public class AudioService
{
    private readonly IAudioManager am = AudioManager.Current;

    public async Task ClickAsync(bool enabled)
    {
        if (!enabled) return;
        using var player = am.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("click.mp3"));
        player.Play();
    }
}
