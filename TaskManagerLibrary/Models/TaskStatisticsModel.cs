namespace TaskManagerLibrary.Models;

public class TaskStatisticsModel
{
    public int TotalTasks { get; set; }
    public double CompletionRate { get; set; }
    public Dictionary<TaskState, int> TasksByState { get; set; } = new();
    public Dictionary<TaskPriority, int> TasksByPriority { get; set; } = new();
    public Dictionary<string, int> TasksByCategory { get; set; } = new();
    public double AverageDaysToDeadline { get; set; }
}