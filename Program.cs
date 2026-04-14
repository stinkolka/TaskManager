using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.CommandLine;
using TaskManagerLibrary.Infrastructure;
using TaskManagerLibrary.Interfaces;
using TaskManagerLibrary.Service;
using TaskManagerLibrary.Models;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var builder = new ConfigurationBuilder();
        BuildConfig(builder);
        var config = builder.Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(config)
            .CreateLogger();

        try
        {
            Log.Information("Task Manager CLI is starting...");

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    string storagePath = config.GetValue<string>("TaskStoragePath") ?? "tasks.json";
                    
                    services.AddSingleton<ITaskRepository>(new JsonTaskRepository(storagePath));
                    services.AddSingleton<ITaskService, TaskService>();
                })
                .UseSerilog()
                .Build();
            
            var taskService = host.Services.GetRequiredService<ITaskService>();
            
            return await SetupCommandLine(taskService, args);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
    
    static void BuildConfig(IConfigurationBuilder builder)
    {
        builder.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();
    }
    
    static async Task<int> SetupCommandLine(ITaskService taskService, string[] args)
    {
        var nameArgument = new Argument<string>("name") { Description = "Task description" };
        
        var priorityOption = new Option<TaskPriority>("--priority")
        {
            Description = "Task priority level",
            DefaultValueFactory = _ => TaskPriority.Medium
        };

        var categoryOption = new Option<string>("--category")
        {
            Description = "Task category",
            DefaultValueFactory = _ => "General"
        };

        var deadlineOption = new Option<DateTime?>("--deadline")
        {
            Description = "Due date (yyyy-MM-dd)"
        };

        var idArgument = new Argument<string>("id") { Description = "Task index (e.g. 1) or GUID" };
        
        
        var addCommand = new Command("add", "Add a new task")
        {
            Arguments = { nameArgument },
            Options = { priorityOption, categoryOption, deadlineOption }
        };

        var listCommand = new Command("list", "List all tasks");
        
        var startCommand = new Command("start", "Mark a task as in progress")
        {
            Arguments = { idArgument }
        };

        var completeCommand = new Command("complete", "Mark a task as complete")
        {
            Arguments = { idArgument }
        };

        var statsCommand = new Command("stats", "Show task statistics");
        
        
        var rootCommand = new RootCommand("Task Manager CLI")
        {
            Subcommands = { addCommand, listCommand, startCommand, completeCommand, statsCommand }
        };
        

        addCommand.SetAction(async parseResult => 
        {
            var name = parseResult.GetValue(nameArgument)!;
            var priority = parseResult.GetValue(priorityOption);
            var category = parseResult.GetValue(categoryOption)!;
            var deadline = parseResult.GetValue(deadlineOption);
            
            await taskService.AddTaskAsync(name, priority, category, deadline);
    
            Console.WriteLine($"Task '{name}' added successfully.");
        });

        listCommand.SetAction(async parseResult =>
        {
            var tasks = await taskService.GetAllTasksAsync();
            var sortedTasks = tasks.OrderByDescending(t => t.CreatedAt).ToList();
            if (tasks.Count == 0)
            {
                Console.WriteLine("No tasks found.");
                return;
            }
            
            int index = 1; 
            foreach (var t in sortedTasks)
            {
                string status = t.State switch
                {
                    TaskState.Done => "✓",
                    TaskState.InProgress => "~",
                    _ => " "
                };
                
                string dateStr = t.CreatedAt.ToString("dd.MM.");
    
                Console.WriteLine($"{index}. [{status}] {t.Name} (Vytvorené: {dateStr}, Priorita: {t.Priority})");
                index++;
            }
        });
        
        startCommand.SetAction(async parseResult =>
        {
            var input = parseResult.GetValue(idArgument);
            var tasks = await taskService.GetAllTasksAsync();

            if (int.TryParse(input, out int taskIndex) && taskIndex > 0 && taskIndex <= tasks.Count)
            {
                var targetId = tasks[taskIndex - 1].Id;
                
                await taskService.UpdateStatusAsync(targetId, TaskState.InProgress);
        
                Console.WriteLine($"Task {taskIndex} is now In Progress!");
            }
            else {
                Console.WriteLine("Invalid task number.");
            }
        });

        completeCommand.SetAction(async parseResult =>
        {
            var input = parseResult.GetValue(idArgument).ToString(); 

            var tasks = await taskService.GetAllTasksAsync();
            
            if (int.TryParse(input, out int taskIndex) && taskIndex > 0 && taskIndex <= tasks.Count)
            {
                var actualId = tasks[taskIndex - 1].Id;
                await taskService.UpdateStatusAsync(actualId, TaskState.Done);
                Console.WriteLine($"Task {taskIndex} was completed.");
            }
            else
            {
                Console.WriteLine("Invalid task number.");
            }
        });

        statsCommand.SetAction(async parseResult =>
        {
            var s = await taskService.GetStatisticsAsync();
            Console.WriteLine("\n--- TASK STATISTICS ---");
            Console.WriteLine($"Total tasks: {s.TotalTasks}");
            Console.WriteLine($"Average days to deadline: {s.AverageDaysToDeadline:F1}");
            Console.WriteLine("\nTasks by State:");
            foreach(var entry in s.TasksByState) Console.WriteLine($"  {entry.Key}: {entry.Value}");
        });
        
        return await rootCommand.Parse(args).InvokeAsync();
    }
}