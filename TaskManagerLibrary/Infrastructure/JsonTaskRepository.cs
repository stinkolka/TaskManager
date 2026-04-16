using System.Text.Json;
using TaskManagerLibrary.Interfaces;
using TaskManagerLibrary.Models;

namespace TaskManagerLibrary.Infrastructure;

public class JsonTaskRepository : ITaskRepository
{
    private readonly string _storageFilePath;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public JsonTaskRepository(string storageFilePath = "tasks.json")
    {
        _storageFilePath = storageFilePath;
        
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true 
        };
    }

    public async Task<List<TaskModel>> LoadTasksAsync()
    {
        if (!File.Exists(_storageFilePath))
        {
            return new List<TaskModel>();
        }

        try
        {
            string rawJsonContent = await File.ReadAllTextAsync(_storageFilePath);
            
            return JsonSerializer.Deserialize<List<TaskModel>>(rawJsonContent, _jsonSerializerOptions)
                   ?? new List<TaskModel>();
        }
        catch (JsonException)
        {
            return new List<TaskModel>();
        }
    }

    public async Task SaveTasksAsync(List<TaskModel> taskList)
    {
        string serializedData = JsonSerializer.Serialize(taskList, _jsonSerializerOptions);
        await File.WriteAllTextAsync(_storageFilePath, serializedData);
    }

    public async Task<bool> DeleteAsync(Guid taskId)
    {
        var currentTasks = await LoadTasksAsync();

        var taskToExclude = currentTasks.FirstOrDefault(t => t.Id == taskId);

        if (taskToExclude == null)
        {
            return false;
        }

        currentTasks.Remove(taskToExclude);
        await SaveTasksAsync(currentTasks);
        
        return true;
    }
}    