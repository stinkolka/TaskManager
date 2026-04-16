using Spectre.Console;
using TaskManagerLibrary.Interfaces;
using TaskManagerLibrary.Models;

namespace TaskManager;

public class TaskCommandHandler
{
    private readonly ITaskService _taskService;
    private readonly ITaskUI _ui;

    public TaskCommandHandler(ITaskService taskService, ITaskUI ui)
    {
        _taskService = taskService;
        _ui = ui;
    }
    
    public async Task List()
    {
        AnsiConsole.Clear();
        var taskList = await _taskService.GetAllTasksAsync();
        _ui.DisplayList(taskList);
    }
    
    public async Task Stats()
    {
        AnsiConsole.Clear();
        var statisticsResult = await _taskService.GetStatisticsAsync();
        _ui.DisplayStats(statisticsResult);
    }

    public async Task Add(string taskName, TaskPriority priority, string categoryName)
    {
        await _taskService.AddTaskAsync(taskName, priority, categoryName, null);
        _ui.ShowSuccess($"Task '{taskName}' added.");
    }

    public async Task Start()
    {
        var allTasks = await _taskService.GetAllTasksAsync();
        var availableToStart = allTasks.Where(t => t.State == TaskState.Todo).ToList();

        var selectedTasks = _ui.SelectTasksToStart(availableToStart);

        if (selectedTasks.Any())
        {
            foreach (var task in selectedTasks)
            {
                await _taskService.UpdateStatusAsync(task.Id, TaskState.InProgress);
            }
            _ui.ShowSuccess($"Started {selectedTasks.Count} tasks.");
        }
    }
    
    public async Task MarkMultipleAsDone()
    {
        var allTasks = await _taskService.GetAllTasksAsync();
        var pendingTasks = allTasks.Where(t => t.State != TaskState.Done).ToList();

        var selectedTasks = _ui.SelectTasksToComplete(pendingTasks);

        if (selectedTasks.Any())
        {
            foreach (var task in selectedTasks)
            {
                await _taskService.UpdateStatusAsync(task.Id, TaskState.Done);
            }
            _ui.ShowSuccess($"Completed {selectedTasks.Count} tasks.");
        }
    }
    
    public async Task Delete()
    {
        var allTasks = await _taskService.GetAllTasksAsync();
        var selectedTasks = _ui.SelectTasksToDelete(allTasks);

        if (selectedTasks.Any() && _ui.ConfirmAction($"Delete {selectedTasks.Count} tasks?"))
        {
            foreach (var task in selectedTasks)
            {
                await _taskService.DeleteTaskAsync(task.Id);
            }
            _ui.ShowSuccess("Tasks deleted.");
        }
    }
    
    public async Task InspectStorage()
    {
        AnsiConsole.Clear();
        string storagePath = "tasks.json"; 
        if (File.Exists(storagePath))
        {
            string rawJsonContent = await File.ReadAllTextAsync(storagePath);
            _ui.DisplayRawData(rawJsonContent);
        }
        else _ui.ShowError("File not found.");
    }
}