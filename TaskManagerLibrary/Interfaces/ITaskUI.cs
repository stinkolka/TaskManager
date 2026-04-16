using TaskManagerLibrary.Models;

namespace TaskManagerLibrary.Interfaces;

public interface ITaskUI
{
    void ShowMessage(string message);
    void ShowSuccess(string message);
    void ShowError(string message);
    void DisplayList(List<TaskModel> tasks);
    void DisplayStats(TaskStatisticsModel stats);
    List<TaskModel> SelectTasksToStart(List<TaskModel> tasks);
    List<TaskModel> SelectTasksToComplete(List<TaskModel> tasks);
    List<TaskModel> SelectTasksToDelete(List<TaskModel> tasks);
    bool ConfirmAction(string message);
    void DisplayRawData(string jsonContent);
}