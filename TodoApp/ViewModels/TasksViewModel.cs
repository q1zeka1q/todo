using System.Collections.ObjectModel;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp.ViewModels;
//управляет списком задач добавляет, удаляет, сортирует, сохраняет и фильтрует задачи
public class TasksViewModel : BaseViewModel
{
    private readonly DatabaseService db;

    // исходные и отфильтрованные
    public ObservableCollection<TaskItem> Asjad { get; } = new();
    public ObservableCollection<TaskItem> Filtreeritud { get; } = new();

    // категории (для выбора и фильтра)
    public ObservableCollection<string> Kategooriad { get; } =
        new(new[] { "Üldine", "Kodu", "Töö", "Õppimine" });

    public ObservableCollection<string> KategooriaFiltrid { get; } =
        new(new[] { "Kõik", "Üldine", "Kodu", "Töö", "Õppimine" });

    public ObservableCollection<string> PrioryFiltrid { get; } =
        new(new[] { "Kõik", "Madal", "Tavaline", "Kõrge" });

    // выбранные значения при добавлении
    private Priory _uusTase = Priory.Normal;
    public Priory UusTase { get => _uusTase; set => Set(ref _uusTase, value); }

    private string _uusKategooria = "Üldine";
    public string UusKategooria { get => _uusKategooria; set => Set(ref _uusKategooria, value); }

    // фильтры
    private string _filterKategooria = "Kõik";
    public string FilterKategooria { get => _filterKategooria; set { Set(ref _filterKategooria, value); ApplyFilter(); } }

    private Priory? _filterTase = null; // null = kõik
    public Priory? FilterTase { get => _filterTase; set { Set(ref _filterTase, value); ApplyFilter(); } }

    public TasksViewModel(DatabaseService db)
    {
        this.db = db;
        Title = "Ülesanded";
    }

    public async Task LaeAsync()
    {
        await db.InitAsync();
        Asjad.Clear();
        foreach (var t in await db.GetAllAsync())
            Asjad.Add(t);
        ApplyFilter();
    }

    public async Task LisaAsync(string title, DateTime? tahtaeg)
    {
        var t = new TaskItem
        {
            Pealkiri = title,
            Tehtud = false,
            Jark = Asjad.Count,
            Tahtaeg = tahtaeg,
            Tase = UusTase,
            Kategooria = UusKategooria
        };
        await db.SaveAsync(t);
        Asjad.Add(t);
        ApplyFilter();
    }

    public async Task ToggleAsync(TaskItem t)
    {
        t.Tehtud = !t.Tehtud;
        await db.SaveAsync(t);
        ApplyFilter();
    }

    public async Task KustutaAsync(TaskItem item)
    {
        await db.DeleteAsync(item);
        Asjad.Remove(item);
        for (int i = 0; i < Asjad.Count; i++) Asjad[i].Jark = i;
        await db.SaveOrderAsync(Asjad);
        ApplyFilter();
    }

    public async Task KustutaKoikAsync()
    {
        await db.DeleteAllAsync();
        Asjad.Clear();
        ApplyFilter();
    }

    public async Task ReorderAsync(TaskItem item, int newIndex)
    {
        Asjad.Remove(item);
        Asjad.Insert(newIndex, item);
        for (int i = 0; i < Asjad.Count; i++) Asjad[i].Jark = i;
        await db.SaveOrderAsync(Asjad);
        ApplyFilter();
    }

    // установить фильтр приоритета по строке из Picker
    public void SetPriorityFilterByString(string s)
    {
        FilterTase = s switch
        {
            "Madal" => Priory.Low,
            "Tavaline" => Priory.Normal,
            "Kõrge" => Priory.High,
            _ => (Priory?)null
        };
    }

    private void ApplyFilter()
    {
        Filtreeritud.Clear();
        IEnumerable<TaskItem> q = Asjad;

        if (FilterKategooria != "Kõik")
            q = q.Where(t => string.Equals(t.Kategooria, FilterKategooria, StringComparison.OrdinalIgnoreCase));

        if (FilterTase.HasValue)
            q = q.Where(t => t.Tase == FilterTase.Value);

        foreach (var i in q)
            Filtreeritud.Add(i);
    }
}
