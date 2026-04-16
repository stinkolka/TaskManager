using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using TaskManager;
using TaskManager.Infrastructure;
using TaskManagerLibrary.Infrastructure;
using TaskManagerLibrary.Interfaces;
using TaskManagerLibrary.Service;


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
                    services.AddSingleton<ITaskUI, ConsoleTaskUI>();
                    
                    services.AddTransient<TaskCommandHandler>();
                })
                .UseSerilog()
                .Build();
            
            var handler = host.Services.GetRequiredService<TaskCommandHandler>();
            
            return await SetupCommandLine(handler, args);
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
    
    static async Task<int> SetupCommandLine(TaskCommandHandler handler, string[] args)
    {
        var builder = new CommandLineBuilder(handler);
        var rootCommand = builder.Build();
        var parseResult = rootCommand.Parse(args);
        
        return await parseResult.InvokeAsync();
    }
}