using System.CommandLine;
using TaskManagerLibrary.Models;

namespace TaskManager;

public class CommandLineBuilder
{
    private readonly TaskCommandHandler _handler;

    public CommandLineBuilder(TaskCommandHandler handler)
    {
        _handler = handler;
    }

    public RootCommand Build()
    {
        var taskNameOption = new Option<string>("--name") { Description = "Task description" };

        var priorityOption = new Option<TaskPriority>("--priority")
        {
            DefaultValueFactory = _ => TaskPriority.Medium,
            Description = "Priority level"
        };

        var categoryOption = new Option<string>("--category")
        {
            DefaultValueFactory = _ => "General",
            Description = "Category"
        };
        
        var addCommand = new Command("add", "Add a new task") { taskNameOption, priorityOption, categoryOption };
        var listCommand = new Command("list", "List all tasks");
        var startCommand = new Command("start", "Interactively select tasks to start");
        var completeCommand = new Command("complete", "Interactively select tasks to complete");
        var statsCommand = new Command("stats", "Show task statistics");
        var inspectCommand = new Command("inspect", "View the raw JSON storage file");
        var deleteCommand = new Command("delete", "Interactively delete tasks");

        addCommand.SetAction(async parseResult =>
        {
            var name = parseResult.GetValue(taskNameOption)!;
            var priority = parseResult.GetValue(priorityOption);
            var category = parseResult.GetValue(categoryOption)!;
            await _handler.Add(name, priority, category);
        });

        listCommand.SetAction(async _ => await _handler.List());
        startCommand.SetAction(async _ => await _handler.Start());
        completeCommand.SetAction(async _ => await _handler.MarkMultipleAsDone());
        statsCommand.SetAction(async _ => await _handler.Stats());
        inspectCommand.SetAction(async _ => await _handler.InspectStorage());
        deleteCommand.SetAction(async _ => await _handler.Delete());

        return new RootCommand("Task Manager CLI")
        {
            addCommand, listCommand, startCommand, completeCommand, statsCommand, inspectCommand, deleteCommand
        };
    }
}