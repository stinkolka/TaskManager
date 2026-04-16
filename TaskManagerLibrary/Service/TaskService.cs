using Microsoft.Extensions.Logging;
using TaskManagerLibrary.Interfaces;
using TaskManagerLibrary.Models;

namespace TaskManagerLibrary.Service;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly ILogger<TaskService> _serviceLogger;

    public TaskService(ITaskRepository taskRepository, ILogger<TaskService> serviceLogger)
    {
        _taskRepository = taskRepository;
        _serviceLogger = serviceLogger;
    }

    public async Task AddTaskAsync(string taskName, TaskPriority priority, string categoryName, DateTime? deadlineDate)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(taskName);

            var taskList = await _taskRepository.LoadTasksAsync();

            var newTask = new TaskModel
            {
                Id = Guid.NewGuid(),
                Name = taskName,
                Priority = priority,
                Category = categoryName,
                Deadline = deadlineDate,
                State = TaskState.Todo,
                CreatedAt = DateTime.UtcNow
            };

            taskList.Add(newTask);
            await _taskRepository.SaveTasksAsync(taskList);

            _serviceLogger.LogInformation("Task with ID {TaskId} and name '{Name}' was successfully added.", newTask.Id,
                taskName);
        }
        catch (Exception ex)
        {
            _serviceLogger.LogError(ex, "Error occurred while adding task with name '{Name}'.", taskName);
            throw;
        }
    }

    public async Task<List<TaskModel>> GetAllTasksAsync()
    {
        _serviceLogger.LogDebug("Loading all tasks from the repository.");
        return await _taskRepository.LoadTasksAsync();
    }

    public async Task UpdateStatusAsync(Guid taskId, TaskState newState)
    {
        try
        {
            var taskList = await _taskRepository.LoadTasksAsync();
            var taskToUpdate = taskList.FirstOrDefault(t => t.Id == taskId);

            if (taskToUpdate == null)
            {
                _serviceLogger.LogWarning("Attempted to update non-existent task with ID {TaskId}.", taskId);
                throw new KeyNotFoundException($"Task with ID {taskId} was not found.");
            }

            var oldState = taskToUpdate.State;
            taskToUpdate.State = newState;

            await _taskRepository.SaveTasksAsync(taskList);

            _serviceLogger.LogInformation("Task {TaskId} state changed from {OldState} to {NewState}.", taskId,
                oldState, newState);
        }
        catch (Exception ex)
        {
            _serviceLogger.LogError(ex, "Error occurred while updating status for task {TaskId}.", taskId);
            throw;
        }
    }

    public async Task CompleteTaskAsync(Guid taskId)
    {
        _serviceLogger.LogInformation("Marking task {TaskId} as completed.", taskId);
        await UpdateStatusAsync(taskId, TaskState.Done);
    }

    public async Task<TaskStatisticsModel> GetStatisticsAsync()
    {
        try
        {
            _serviceLogger.LogInformation("Generating task statistics.");
            var taskList = await _taskRepository.LoadTasksAsync();

            var statisticsResult = new TaskStatisticsModel
            {
                TotalTasks = taskList.Count,
                
                CompletionRate = taskList.Count == 0 
                    ? 0 
                    : (double)taskList.Count(task => task.State == TaskState.Done) / taskList.Count * 100,

                TasksByState = taskList
                    .GroupBy(t => t.State)
                    .ToDictionary(g => g.Key, g => g.Count()),

                TasksByPriority = taskList
                    .GroupBy(t => t.Priority)
                    .ToDictionary(g => g.Key, g => g.Count()),

                TasksByCategory = taskList
                    .GroupBy(t => t.Category ?? "Uncategorized")
                    .ToDictionary(g => g.Key, g => g.Count()),

                AverageDaysToDeadline = taskList
                    .Where(t => t.Deadline.HasValue && t.State != TaskState.Done)
                    .Select(t => (t.Deadline.Value - DateTime.UtcNow).TotalDays)
                    .DefaultIfEmpty(0)
                    .Average()
            };

            return statisticsResult;
        }
        catch (Exception ex)
        {
            _serviceLogger.LogError(ex, "Error occurred while generating statistics.");
            throw;
        }
    }

    public async Task<bool> DeleteTaskAsync(Guid taskId)
    {
        _serviceLogger.LogInformation("Requesting deletion of task {TaskId}.", taskId);
        return await _taskRepository.DeleteAsync(taskId);
    }
}