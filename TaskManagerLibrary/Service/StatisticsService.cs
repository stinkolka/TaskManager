using TaskManagerLibrary.Models;

namespace TaskManagerLibrary.Service;

public class StatisticsService
{
    public TaskStatisticsModel CalculateStatistics(IEnumerable<TaskModel> tasks)
    {
        var taskList = tasks.ToList();
        var total = taskList.Count;

        return new TaskStatisticsModel
        {
            TotalTasks = total,
            TasksByState = taskList.GroupBy(t => t.State)
                .ToDictionary(g => g.Key, g => g.Count()),
            TasksByPriority = taskList.GroupBy(t => t.Priority)
                .ToDictionary(g => g.Key, g => g.Count()),
            TasksByCategory = taskList.GroupBy(t => t.Category)
                .ToDictionary(g => g.Key, g => g.Count()),
            CompletionRate = total == 0 ? 0 : 
                (double)taskList.Count(t => t.State == TaskState.Done) / total
        };
    }
}