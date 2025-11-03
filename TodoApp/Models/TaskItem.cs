using SQLite;

namespace TodoApp.Models;

// одна задача (что нужно сделать) хранит название, дату, время, приоритет, категорию и т.д.
public enum Priory { Low = 0, Normal = 1, High = 2 }

[Table("TaskItem")]
public class TaskItem
{
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    public DateTime Kuupaev { get; set; } = DateTime.Now; // создано
    public string Pealkiri { get; set; } = "";            // заголовок
    public bool Tehtud { get; set; }                      // выполнено
    public int Jark { get; set; }                         // порядок
    public DateTime? Tahtaeg { get; set; }                // дедлайн

    // новое
    public Priory Tase { get; set; } = Priory.Normal;     // приоритет
    public string Kategooria { get; set; } = "Üldine";    // категория
}
