using TaskManagerLibrary.Interfaces;
using TaskManagerLibrary.Models;
using Microsoft.Extensions.Logging;

namespace TaskManagerLibrary.Service;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;
    private readonly ILogger<TaskService> _logger;
    
    public TaskService(ITaskRepository repository, ILogger<TaskService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task AddTaskAsync(string name, TaskPriority priority, string category, DateTime? deadline)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
            
            var loadedTasks = await _repository.LoadTasksAsync();
            
            var newTask = new TaskModel
            {
                Id = Guid.NewGuid(),
                Name = name,
                Priority = priority,
                Category = category,
                Deadline = deadline,
                State = TaskState.Todo,
                CreatedAt = DateTime.UtcNow
            };
            
            loadedTasks.Add(newTask);
            await _repository.SaveTasksAsync(loadedTasks);
            
            _logger.LogInformation("Task with ID {TaskId} and name '{Name}' was successfully added.", newTask.Id, name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding task with name '{Name}'.", name);
            throw;
        }
    }

    public async Task<List<TaskModel>> GetAllTasksAsync()
    {
        _logger.LogDebug("Loading all tasks from the repository.");
        return await _repository.LoadTasksAsync();
    }

    public async Task UpdateStatusAsync(Guid id, TaskState state)
    {
        try
        {
            var loadedTasks = await _repository.LoadTasksAsync();
            var taskToUpdate = loadedTasks.FirstOrDefault(t => t.Id == id);

            if (taskToUpdate == null)
            {
                _logger.LogWarning("Attempted to update non-existent task with ID {TaskId}.", id);
                throw new KeyNotFoundException($"Task with ID {id} was not found.");
            }

            var oldState = taskToUpdate.State;
            taskToUpdate.State = state;
            
            await _repository.SaveTasksAsync(loadedTasks);
            
            _logger.LogInformation("Task {TaskId} state changed from {OldState} to {NewState}.", id, oldState, state);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating status for task {TaskId}.", id);
            throw;
        }
    }

    public async Task CompleteTaskAsync(Guid id)
    {
        _logger.LogInformation("Marking task {TaskId} as completed.", id);
        await UpdateStatusAsync(id, TaskState.Done);
    }

    public async Task<TaskStatisticsModel> GetStatisticsAsync()
    {
        try
        {
            _logger.LogInformation("Generating task statistics.");
            _logger.LogDebug("Loading all tasks from the repository.");
            var loadedTasks = await _repository.LoadTasksAsync();
            
            var taskStatistics = new TaskStatisticsModel
            {
                TotalTasks = loadedTasks.Count,
                
                TasksByState = loadedTasks
                    .GroupBy(t => t.State)
                    .ToDictionary(g => g.Key, g => g.Count()),

                TasksByPriority = loadedTasks
                    .GroupBy(t => t.Priority)
                    .ToDictionary(g => g.Key, g => g.Count()),
                
                TasksByCategory = loadedTasks
                    .GroupBy(t => t.Category ?? "Uncategorized")
                    .ToDictionary(g => g.Key, g => g.Count()),
                
                AverageDaysToDeadline = loadedTasks
                    .Where(t => t.Deadline.HasValue && t.State != TaskState.Done)
                    .Select(t => (t.Deadline.Value - DateTime.UtcNow).TotalDays)
                    .DefaultIfEmpty(0)
                    .Average()
            };

            return taskStatistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while generating statistics.");
            throw;
        }
    }
}