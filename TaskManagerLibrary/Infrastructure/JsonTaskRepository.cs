using System.Text.Json;
using TaskManagerLibrary.Interfaces;
using TaskManagerLibrary.Models;

namespace TaskManagerLibrary.Infrastructure;

public class JsonTaskRepository : ITaskRepository
{
    private readonly string _filePath;
    public JsonTaskRepository(string filePath = "tasks.json")
    {
        _filePath = filePath;
    }
    
    public async Task<List<TaskModel>> LoadTasksAsync()
    {
        if (!File.Exists(_filePath))
        {
            return new List<TaskModel>();
        }

        try
        {
            string fileContent = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<TaskModel>>(fileContent) ?? new List<TaskModel>();
        }
        catch(JsonException)
        {
            return new List<TaskModel>();
        }
    }

    public async Task SaveTasksAsync(List<TaskModel> tasks)
    {
        var formattingOption = new JsonSerializerOptions { WriteIndented = true };
        string serializedTasks = JsonSerializer.Serialize(tasks, formattingOption);

        await File.WriteAllTextAsync(_filePath, serializedTasks);
    }
}