using TaskManagerLibrary.Models;

namespace TaskManagerLibrary.Interfaces;

public interface ITaskRepository
{
    Task<List<TaskModel>> LoadTasksAsync();
    Task SaveTasksAsync(List<TaskModel> tasks);
}