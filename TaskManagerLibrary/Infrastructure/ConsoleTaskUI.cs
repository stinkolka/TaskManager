using TaskManagerLibrary.Interfaces;
using TaskManagerLibrary.Models;
using Spectre.Console;
using Spectre.Console.Json;

namespace TaskManager.Infrastructure;

public class ConsoleTaskUI : ITaskUI
{
    public void ShowMessage(string message)
    {
        AnsiConsole.MarkupLine($"[white]{message}[/]");
    }

    public void ShowSuccess(string message)
    {
        AnsiConsole.MarkupLine($"[green]SUCCESS:[/] {message}");
    }

    public void ShowError(string message)
    {
        AnsiConsole.MarkupLine($"[red]ERROR:[/] {message}");
    }
    
    public List<TaskModel> SelectTasksToStart(List<TaskModel> availableTasks)
    {
        return SelectMultipleTasks(availableTasks, "START", "orange1");
    }

    public List<TaskModel> SelectTasksToComplete(List<TaskModel> availableTasks)
    {
        return SelectMultipleTasks(availableTasks, "COMPLETE", "green");
    }

    public List<TaskModel> SelectTasksToDelete(List<TaskModel> availableTasks)
    {
        return SelectMultipleTasks(availableTasks, "DELETE", "red");
    }

    private List<TaskModel> SelectMultipleTasks(List<TaskModel> tasksToChooseFrom, string actionName, string color)
    {
        if (!tasksToChooseFrom.Any())
        {
            AnsiConsole.MarkupLine($"[yellow]No tasks available to {actionName.ToLower()}![/]");
            return new List<TaskModel>();
        }

        var selectedTasks = AnsiConsole.Prompt(
            new MultiSelectionPrompt<TaskModel>()
                .Title($"Which tasks do you want to [{color}]{actionName}[/]?")
                .NotRequired()
                .PageSize(10)
                .InstructionsText("[grey](Press [blue]<space>[/] to toggle, [green]<enter>[/] to confirm)[/]")
                .AddChoices(tasksToChooseFrom) 
                .UseConverter(task => $"{task.Name} [[{task.Priority}]]")
        );

        return selectedTasks;
    }
    
    private string GetPriorityColor(TaskPriority priority)
    {
        return priority switch
        {
            TaskPriority.Critical => "maroon",
            TaskPriority.High => "red",
            TaskPriority.Medium => "yellow",
            TaskPriority.Low => "blue",
            _ => "white"
        };
    }
    
    public void DisplayList(List<TaskModel> taskList)
    {
        if (!taskList.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No tasks found. Your list is empty![/]");
            return;
        }

        var taskGrid = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .Title("[bold white]YOUR TASK LIST[/]");

        taskGrid.AddColumn(new TableColumn("[bold yellow]#[/]").Centered());
        taskGrid.AddColumn(new TableColumn("[bold]Task Name[/]"));
        taskGrid.AddColumn(new TableColumn("[bold]Priority[/]").Centered());
        taskGrid.AddColumn(new TableColumn("[bold]State[/]").Centered());
        taskGrid.AddColumn(new TableColumn("[bold]Deadline[/]").Centered());

        int taskRowIdentifier = 1;
    
        foreach (var task in taskList)
        {
            string priorityStyleColor = GetPriorityColor(task.Priority);

            string stateVisualRepresentation = task.State switch
            {
                TaskState.Done => "[green]✔ Done[/]",
                TaskState.InProgress => "[orange1]In Progress[/]",
                _ => "[grey]TODO[/]"
            };

            string formattedDeadline = task.Deadline?.ToString("yyyy-MM-dd") ?? "[grey]N/A[/]";

            taskGrid.AddRow(
                taskRowIdentifier.ToString(),
                task.Name,
                $"[{priorityStyleColor}]{task.Priority}[/]",
                stateVisualRepresentation,
                formattedDeadline
            );
        
            taskRowIdentifier++;
        }

        AnsiConsole.Write(taskGrid);
    }
    
    public void DisplayStats(TaskStatisticsModel statisticsData)
    {
        AnsiConsole.Write(new Rule("[yellow]Task Manager Statistics[/]").Justify(Justify.Left));
        AnsiConsole.WriteLine();
        
        var summaryMetricsTable = new Table().Border(TableBorder.Rounded).BorderColor(Color.Blue);
        summaryMetricsTable.AddColumn("[bold]Metric[/]");
        summaryMetricsTable.AddColumn("[bold]Value[/]");
        summaryMetricsTable.AddRow("Total Tasks", $"[white]{statisticsData.TotalTasks}[/]");
        
        string completionRateColor = statisticsData.CompletionRate >= 80 ? "green" : "yellow";
        summaryMetricsTable.AddRow("Completion Rate", $"[{completionRateColor}]{statisticsData.CompletionRate:F1}%[/]");
        summaryMetricsTable.AddRow("Avg. Days to Deadline", $"[blue]{statisticsData.AverageDaysToDeadline:F1} days[/]");

        AnsiConsole.Write(summaryMetricsTable);
        
        var detailedBreakdownTable = new Table().Border(TableBorder.Square).BorderColor(Color.Grey);
        detailedBreakdownTable.AddColumn("[bold]Category / Priority[/]");
        detailedBreakdownTable.AddColumn("[bold]Count[/]");
        
        foreach (var stateDistribution in statisticsData.TasksByState)
        {
            string stateColor = stateDistribution.Key == TaskState.Done ? "green" : "orange1";
            detailedBreakdownTable.AddRow($"State: [italic]{stateDistribution.Key}[/]", $"[{stateColor}]{stateDistribution.Value}[/]");
        }

        detailedBreakdownTable.AddEmptyRow();
        
        foreach (var priorityDistribution in statisticsData.TasksByPriority)
        {
            string priorityStyleColor = GetPriorityColor(priorityDistribution.Key);
            detailedBreakdownTable.AddRow($"Priority: [{priorityStyleColor}]{priorityDistribution.Key}[/]", priorityDistribution.Value.ToString());
        }

        AnsiConsole.Write(detailedBreakdownTable);
    }
    
    public bool ConfirmAction(string message)
    {
        return AnsiConsole.Confirm(message);
    }
    
    public void DisplayRawData(string rawJsonContent)
    {
        var formattedJson = new JsonText(rawJsonContent);
        
        formattedJson.StringColor(Color.Yellow);
        formattedJson.ColonColor(Color.Orange1);
        formattedJson.BracketColor(Color.Green);

        AnsiConsole.Write(
            new Panel(formattedJson)
                .Header(" [bold blue]Storage Inspection (tasks.json)[/] ")
                .Collapse()
                .BorderColor(Color.White)
                .RoundedBorder()
        );
    }
}