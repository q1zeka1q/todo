using SQLite;
using TodoApp.Models;

namespace TodoApp.Services;
//сохраняет все задачи в базу данных загружает, удаляет и обновляет их
// простая обёртка над SQLite
public class DatabaseService
{
    private SQLiteAsyncConnection db = null!;          // инициализируется в InitAsync
    private const string DbName = "todo.db";

    public async Task InitAsync()
    {
        if (db != null) return;

        var path = Path.Combine(FileSystem.AppDataDirectory, DbName);
        db = new SQLiteAsyncConnection(path);
        await db.CreateTableAsync<TaskItem>();

        //  пробуем добавить столбцы, если их нет
        try { await db.ExecuteAsync("ALTER TABLE TaskItem ADD COLUMN Tase INTEGER NOT NULL DEFAULT 1"); } catch { }
        try { await db.ExecuteAsync("ALTER TABLE TaskItem ADD COLUMN Kategooria TEXT NOT NULL DEFAULT 'Üldine'"); } catch { }
    }

    public Task<List<TaskItem>> GetAllAsync() =>
        db.Table<TaskItem>().OrderBy(t => t.Jark).ToListAsync();

    public Task<int> SaveAsync(TaskItem item) =>
        item.Id == 0 ? db.InsertAsync(item) : db.UpdateAsync(item);

    public Task<int> DeleteAsync(TaskItem item) => db.DeleteAsync(item);

    public async Task SaveOrderAsync(IEnumerable<TaskItem> items)
    {
        await db.RunInTransactionAsync(tr =>
        {
            foreach (var i in items) tr.Update(i);
        });
    }

    public Task DeleteAllAsync() => db.DeleteAllAsync<TaskItem>();
}
