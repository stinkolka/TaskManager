namespace TaskManagerLibrary.Models;

public class TaskModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public TaskPriority Priority { get; set; }
    public TaskState State { get; set; } = TaskState.Todo;
    public DateTime? Deadline { get; set; }
    public string Category { get; set; } = "General";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}