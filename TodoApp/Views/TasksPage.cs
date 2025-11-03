using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using TodoApp.Models;
using TodoApp.ViewModels;

namespace TodoApp.Views;
// Страница управления задачами: добавление, удаление, фильтрация, Drag & Drop
public class TasksPage : ContentPage
{
    private readonly Entry titleEntry;
    private readonly DatePicker datePicker;
    private readonly Picker hourPicker;     // "hh", 00..23
    private readonly Picker minutePicker;   // "mm", 00..59

    private readonly Picker priPicker;      // "Prioriteet", Madal/…/Kõrge
    private readonly Picker catPicker;      // "Kategooria", …

    private readonly Picker filterCat;      // фильтр категории
    private readonly Picker filterPri;      // фильтр приоритета

    private TaskItem? dragged;

    public TasksPage()
    {
        Title = "Ülesanded";
        var vm = App.Current?.Handler?.MauiContext?.Services.GetService<TasksViewModel>();
        BindingContext = vm;

        // ---------- ресурсы/стили ----------
        var rd = new ResourceDictionary();
        var cardBg = Application.Current!.RequestedTheme == AppTheme.Dark
            ? Color.FromArgb("#232329") : Color.FromArgb("#FFFFFF");
        var surface = Application.Current!.RequestedTheme == AppTheme.Dark
            ? Color.FromArgb("#1F1F23") : Color.FromArgb("#F7F7FB");
        var textMuted = Application.Current!.RequestedTheme == AppTheme.Dark
            ? Color.FromArgb("#9CA3AF") : Color.FromArgb("#6B7280");

        rd["Primary"] = Color.FromArgb("#4F46E5");
        rd["Danger"] = Color.FromArgb("#EF4444");
        rd["Amber"] = Color.FromArgb("#F59E0B");
        rd["Green"] = Color.FromArgb("#10B981");
        rd["CardBg"] = cardBg;
        rd["Surface"] = surface;
        rd["TextMuted"] = textMuted;

        rd["CardStyle"] = new Style(typeof(Border))
        {
            Setters =
            {
                new Setter { Property = Border.PaddingProperty,     Value = new Thickness(14) },
                new Setter { Property = Border.BackgroundProperty,  Value = new SolidColorBrush(cardBg) },
                new Setter { Property = Border.StrokeShapeProperty, Value = new RoundRectangle{ CornerRadius = 18 } },
                new Setter { Property = Border.ShadowProperty,      Value = new Shadow{ Radius = 12, Offset = new Point(0,6), Opacity = 0.30f } },
            }
        };
        rd["BtnPrimary"] = new Style(typeof(Button))
        {
            Setters =
            {
                new Setter { Property = Button.BackgroundColorProperty, Value = rd["Primary"] },
                new Setter { Property = Button.TextColorProperty,       Value = Colors.White },
                new Setter { Property = Button.CornerRadiusProperty,    Value = 14 },
                new Setter { Property = Button.PaddingProperty,         Value = new Thickness(18,10) },
                new Setter { Property = Button.HeightRequestProperty,   Value = 46d }
            }
        };
        rd["BtnDanger"] = new Style(typeof(Button))
        {
            BasedOn = (Style)rd["BtnPrimary"],
            Setters = { new Setter { Property = Button.BackgroundColorProperty, Value = rd["Danger"] } }
        };
        var chipBg = Application.Current!.RequestedTheme == AppTheme.Dark
            ? Color.FromArgb("#2A2E47") : Color.FromArgb("#EEF2FF");
        rd["Chip"] = new Style(typeof(Border))
        {
            Setters =
            {
                new Setter { Property = Border.PaddingProperty,     Value = new Thickness(8,4) },
                new Setter { Property = Border.BackgroundProperty,  Value = new SolidColorBrush(chipBg) },
                new Setter { Property = Border.StrokeShapeProperty, Value = new RoundRectangle{ CornerRadius = 12 } },
            }
        };

        Resources = rd;

        // ---------- заголовок ----------
        var h1 = new Label { Text = "Ülesanded", FontSize = 28, FontAttributes = FontAttributes.Bold };

        // ---------- панель добавления ----------
        titleEntry = new Entry { Placeholder = "Kirjuta plaan...", HeightRequest = 46, Margin = new Thickness(0, 6) };
        datePicker = new DatePicker { Date = DateTime.Today, HeightRequest = 46, WidthRequest = 160, Margin = new Thickness(0, 6) };

        hourPicker = new Picker { HeightRequest = 46, WidthRequest = 90, Margin = new Thickness(0, 6) };
        hourPicker.Items.Add("hh");
        for (int h = 0; h <= 23; h++) hourPicker.Items.Add(h.ToString("00"));
        hourPicker.SelectedIndex = 0;

        minutePicker = new Picker { HeightRequest = 46, WidthRequest = 90, Margin = new Thickness(0, 6) };
        minutePicker.Items.Add("mm");
        for (int m = 0; m <= 59; m++) minutePicker.Items.Add(m.ToString("00"));
        minutePicker.SelectedIndex = 0;

        priPicker = new Picker { HeightRequest = 46, WidthRequest = 140, Margin = new Thickness(0, 6) };
        priPicker.ItemsSource = new[] { "Prioriteet", "Madal", "Tavaline", "Kõrge" };
        priPicker.SelectedIndex = 0;
        priPicker.SelectedIndexChanged += (_, __) =>
        {
            if (BindingContext is not TasksViewModel vm2) return;
            vm2.UusTase = priPicker.SelectedIndex switch
            {
                1 => Priory.Low,
                3 => Priory.High,
                _ => Priory.Normal
            };
        };

        catPicker = new Picker { HeightRequest = 46, WidthRequest = 160, Margin = new Thickness(0, 6) };

        var addBtn = new Button { Text = "Lisa", Style = (Style)rd["BtnPrimary"] };
        var clearAllBtn = new Button { Text = "Kustuta kõik", Style = (Style)rd["BtnDanger"] };
        addBtn.Clicked += OnAdd;
        clearAllBtn.Clicked += OnClearAll;

        var input = new FlexLayout
        {
            Direction = FlexDirection.Row,
            Wrap = FlexWrap.Wrap,
            AlignItems = FlexAlignItems.Center,
            JustifyContent = FlexJustify.Start,
            Margin = new Thickness(0, 4)
        };
        FlexLayout.SetGrow(titleEntry, 1);
        FlexLayout.SetBasis(titleEntry, new FlexBasis(0, true));
        input.Children.Add(titleEntry);
        input.Children.Add(datePicker);
        input.Children.Add(hourPicker);
        input.Children.Add(minutePicker);
        input.Children.Add(priPicker);
        input.Children.Add(catPicker);
        input.Children.Add(addBtn);
        input.Children.Add(clearAllBtn);

        var inputCard = new Border { Style = (Style)rd["CardStyle"], Content = input, Margin = new Thickness(0, 6, 0, 10) };

        // ---------- фильтры ----------
        filterCat = new Picker { HeightRequest = 40, WidthRequest = 160, Margin = new Thickness(0, 6) };
        filterCat.SetBinding(Picker.ItemsSourceProperty, nameof(TasksViewModel.KategooriaFiltrid));
        filterCat.SelectedIndexChanged += (_, __) =>
        {
            if (BindingContext is TasksViewModel vmm && filterCat.SelectedIndex >= 0)
                vmm.FilterKategooria = vmm.KategooriaFiltrid[filterCat.SelectedIndex];
        };

        filterPri = new Picker { HeightRequest = 40, WidthRequest = 140, Margin = new Thickness(8, 6, 0, 6) };
        filterPri.SetBinding(Picker.ItemsSourceProperty, nameof(TasksViewModel.PrioryFiltrid));
        filterPri.SelectedIndexChanged += (_, __) =>
        {
            if (BindingContext is TasksViewModel vmm)
            {
                var s = (string?)filterPri.SelectedItem ?? "Kõik";
                vmm.SetPriorityFilterByString(s);
            }
        };

        var filtersRow = new FlexLayout
        {
            Direction = FlexDirection.Row,
            Wrap = FlexWrap.Wrap,
            AlignItems = FlexAlignItems.Center,
            Children =
            {
                new Label{ Text="Näita:", FontAttributes=FontAttributes.Bold, Margin=new Thickness(0,0,10,0)},
                filterCat, filterPri
            }
        };
        var filtersCard = new Border { Style = (Style)rd["CardStyle"], Content = filtersRow };

        // ---------- список ----------
        var listBorder = new Border
        {
            Background = new SolidColorBrush((Color)rd["Surface"]),
            StrokeShape = new RoundRectangle { CornerRadius = 18 },
            Padding = 0,
            Shadow = new Shadow { Radius = 14, Opacity = 0.18f, Offset = new Point(0, 6) }
        };

        var list = new CollectionView
        {
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical) { ItemSpacing = 6 }
        };
        list.SetBinding(ItemsView.ItemsSourceProperty, nameof(TasksViewModel.Filtreeritud));
        list.EmptyView = new Label
        {
            Text = "Siin on tühi. Lisa uus ülesanne ↑",
            HorizontalTextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 40, 0, 40),
            TextColor = Colors.Gray
        };

        list.ItemTemplate = new DataTemplate(() =>
        {
            Color PrioColor(Priory p) => p switch
            {
                Priory.Low => (Color)rd["Green"],
                Priory.High => (Color)rd["Danger"],
                _ => (Color)rd["Amber"]
            };

            var prioBar = new BoxView { WidthRequest = 4, HorizontalOptions = LayoutOptions.Start, CornerRadius = 2 };
            prioBar.SetBinding(BoxView.ColorProperty,
                new Binding(nameof(TaskItem.Tase), converter: new FuncConverter<Priory, Color>(PrioColor)));

            var title = new Label { FontSize = 16, FontAttributes = FontAttributes.Bold };
            title.SetBinding(Label.TextProperty, nameof(TaskItem.Pealkiri));

            var meta = new Label { FontSize = 12, TextColor = (Color)rd["TextMuted"] };
            meta.SetBinding(Label.TextProperty,
                new Binding(nameof(TaskItem.Kuupaev), stringFormat: "Loodud: {0:dd.MM.yyyy HH:mm}"));

            var deadline = new Label { FontSize = 12, TextColor = (Color)rd["Amber"] };
            deadline.SetBinding(Label.TextProperty,
                new Binding(nameof(TaskItem.Tahtaeg), stringFormat: "Tähtaeg: {0:dd.MM.yyyy HH:mm}") { TargetNullValue = "" });

            var chipCat = new Border { Style = (Style)rd["Chip"], Content = new Label { FontSize = 12 } };
            ((Label)chipCat.Content).SetBinding(Label.TextProperty, new Binding(nameof(TaskItem.Kategooria)));

            var chipPrio = new Border { Style = (Style)rd["Chip"], Content = new Label { FontSize = 12 } };
            ((Label)chipPrio.Content).SetBinding(Label.TextProperty, new Binding(nameof(TaskItem.Tase)));
            ((Label)chipPrio.Content).SetBinding(Label.TextColorProperty,
                new Binding(nameof(TaskItem.Tase), converter: new FuncConverter<Priory, Color>(PrioColor)));

            var chips = new HorizontalStackLayout { Spacing = 6, Children = { chipCat, chipPrio } };

            var center = new VerticalStackLayout { Spacing = 2, Children = { title, meta, deadline, chips } };

            // ✅ ФИКС ЧЕКБОКСА: двусторонний биндинг, без ручного инверта/обработчика
            var cb = new CheckBox { VerticalOptions = LayoutOptions.Start };
            cb.SetBinding(CheckBox.IsCheckedProperty, new Binding(nameof(TaskItem.Tehtud), mode: BindingMode.TwoWay));

            var delBtn = new Button
            {
                Text = "🗑",
                BackgroundColor = Colors.Transparent,
                TextColor = (Color)rd["Danger"],
                Padding = new Thickness(6, 0),
                WidthRequest = 36
            };
            delBtn.SetBinding(Button.CommandParameterProperty, new Binding("."));
            delBtn.Clicked += OnDeleteButton;

            var dragHandle = new Label { Text = "↕", FontSize = 18, VerticalTextAlignment = TextAlignment.Center };
            var dragRec = new DragGestureRecognizer { CanDrag = true };
            dragRec.DragStarting += OnDragStarting;
            dragHandle.GestureRecognizers.Add(dragRec);

            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Auto),   // prio bar
                    new ColumnDefinition(GridLength.Auto),   // checkbox
                    new ColumnDefinition(GridLength.Star),   // content
                    new ColumnDefinition(GridLength.Auto),   // delete
                    new ColumnDefinition(GridLength.Auto)    // drag
                },
                ColumnSpacing = 12,
                Padding = new Thickness(12, 10)
            };
            grid.Add(prioBar, 0, 0);
            grid.Add(cb, 1, 0);
            grid.Add(center, 2, 0);
            grid.Add(delBtn, 3, 0);
            grid.Add(dragHandle, 4, 0);

            var drop = new DropGestureRecognizer { AllowDrop = true };
            drop.Drop += OnDrop;
            grid.GestureRecognizers.Add(drop);

            // свайп для удаления (по желанию)
            var swipeDelete = new SwipeItem { Text = "Kustuta", BackgroundColor = (Color)rd["Danger"] };
            swipeDelete.Invoked += OnDeleteSwipe;

            var right = new SwipeItems { swipeDelete };
            right.Mode = SwipeMode.Reveal;   // 👈 свойство задаём отдельно

            return new SwipeView
            {
                RightItems = right,
                Content = new Border { Style = (Style)rd["CardStyle"], Content = grid, Margin = new Thickness(0, 4) }
            };


        });

        listBorder.Content = list;

        // ---------- вся страница ----------
        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 16,
                Spacing = 10,
                Children = { h1, inputCard, filtersCard, listBorder }
            }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TasksViewModel vm)
        {
            await vm.LaeAsync();

            // категории с placeholder'ом
            var cats = new List<string> { "Kategooria" };
            cats.AddRange(vm.Kategooriad);
            catPicker.ItemsSource = cats;
            catPicker.SelectedIndex = 0;
            catPicker.SelectedIndexChanged += (_, __) =>
            {
                if (catPicker.SelectedIndex > 0)
                    vm.UusKategooria = cats[catPicker.SelectedIndex];
                else
                    vm.UusKategooria = "Üldine";
            };

            filterCat.SelectedIndex = vm.KategooriaFiltrid.IndexOf("Kõik");
            filterPri.SelectedIndex = vm.PrioryFiltrid.IndexOf("Kõik");
        }
    }

    // Добавить
    private void OnAdd(object? sender, EventArgs e)
    {
        if (BindingContext is not TasksViewModel vm) return;

        var text = titleEntry.Text?.Trim();
        if (string.IsNullOrEmpty(text))
            text = $"Uus {vm.Asjad.Count + 1}";

        DateTime? tahtaeg = null;
        if (hourPicker.SelectedIndex > 0 && minutePicker.SelectedIndex > 0)
        {
            var h = hourPicker.SelectedIndex - 1;
            var m = minutePicker.SelectedIndex - 1;
            tahtaeg = datePicker.Date.Date + new TimeSpan(h, m, 0);
        }

        _ = vm.LisaAsync(text, tahtaeg);

        // reset
        titleEntry.Text = string.Empty;
        datePicker.Date = DateTime.Today;
        hourPicker.SelectedIndex = 0;
        minutePicker.SelectedIndex = 0;
        priPicker.SelectedIndex = 0;
        catPicker.SelectedIndex = 0;
    }

    // delete (кнопка)
    private async void OnDeleteButton(object? sender, EventArgs e)
    {
        if (BindingContext is not TasksViewModel vm) return;
        if (sender is not Button btn || btn.CommandParameter is not TaskItem t) return;

        var ok = await DisplayAlert("Kustuta ülesanne", $"Kas kustutada '{t.Pealkiri}'?", "Jah", "Ei");
        if (ok) await vm.KustutaAsync(t);
    }

    // delete (свайп)
    private async void OnDeleteSwipe(object? sender, EventArgs e)
    {
        if (BindingContext is not TasksViewModel vm) return;
        var t = (sender as Element)?.BindingContext as TaskItem;
        if (t is null) return;

        var ok = await DisplayAlert("Kustuta ülesanne", $"Kas kustutada '{t.Pealkiri}'?", "Jah", "Ei");
        if (ok) await vm.KustutaAsync(t);
    }

    // Drag & Drop
    private void OnDragStarting(object? sender, DragStartingEventArgs e)
        => dragged = (sender as Element)?.BindingContext as TaskItem;

    private async void OnDrop(object? sender, DropEventArgs e)
    {
        if (BindingContext is not TasksViewModel vm || dragged is null) return;
        if (sender is not Element el || el.BindingContext is not TaskItem target || target == dragged) return;

        var newIndex = vm.Asjad.IndexOf(target);
        await vm.ReorderAsync(dragged, newIndex);
        dragged = null;
    }

    // Удалить всё
    private async void OnClearAll(object? sender, EventArgs e)
    {
        if (BindingContext is not TasksViewModel vm) return;
        var ok = await DisplayAlert("Kustuta kõik", "Kas kustutada kõik ülesanned?", "Jah", "Ei");
        if (ok) await vm.KustutaKoikAsync();
    }
}

// простой конвертер
file sealed class FuncConverter<TSrc, TDest> : IValueConverter
{
    private readonly Func<TSrc, TDest> _f;
    public FuncConverter(Func<TSrc, TDest> f) => _f = f;
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is TSrc v ? _f(v) : default;
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
