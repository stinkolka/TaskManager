using TaskManagerLibrary.Models;

namespace TaskManagerLibrary.Interfaces;

public interface ITaskService
{
    Task AddTaskAsync(string name, TaskPriority priority, string category, DateTime? deadline);
    Task<List<TaskModel>> GetAllTasksAsync();
    Task UpdateStatusAsync(Guid id, TaskState state);
    Task CompleteTaskAsync(Guid id);
    Task<TaskStatisticsModel> GetStatisticsAsync();
    Task<bool> DeleteTaskAsync(Guid id);
}