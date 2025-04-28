using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Diagnostics;
using System.Security.AccessControl;

namespace ConsoleCallee;

internal class Program
{
    static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        var url = configuration["ServerUrl"];

        var serverUrl = args.Length > 0 ? args[0] : "http://localhost:5261/controlhub";

        var connection = new HubConnectionBuilder()
            .WithUrl(serverUrl, options =>
            {
                options.HttpMessageHandlerFactory = (message) =>
                {
                    if (message is HttpClientHandler clientHandler)
                    {
                        clientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                    }
                    return message;
                };
            })
            .Build();

        connection.On<string>("InvokeAction", async (action) =>
        {
            Log.Information($"Received {action}. Launching Notepad...");

            var result = await Task.Run(() => TryLaunchNotepad(action));

            result
                .Tap(() => Log.Information("Notepad launched successfully."))
                .TapError(error => Log.Error("Error launching Notepad: {Error}", error));
        });

        var identity = Environment.MachineName;

        connection.Closed += async (error) =>
        {
            int retryDelay = 1000; // start with 1 second
            while (true)
            {
                try
                {
                    await connection.StartAsync();
                    await Register(connection, identity);
                    Log.Information($"{identity} Reconnected to server on {connection.ConnectionId}.");
                    break;
                }
                catch
                {
                    Log.Warning("Reconnect failed, retrying in {Delay}ms...", retryDelay);
                    await Task.Delay(retryDelay);
                    retryDelay = Math.Min(retryDelay * 2, 30000); // exponential up to 30 sec
                }
            }
        };

        // On reconnected (optional, more reliable)
        connection.Reconnected += async (connectionId) =>
        {
            Log.Information("Reconnected with new connectionId: {ConnectionId}", connectionId);
            await Register(connection, identity);
        };

        await ReconnectLoopAsync(connection, identity);

        await Register(connection, identity);
        Log.Information($"Connected to server. ConnectionId: {connection.ConnectionId} with name: {identity}");

        await Task.Delay(Timeout.Infinite);
    }

    // When initially connected
    static async Task Register(HubConnection connection, string identity)
    {
        try
        {
            await connection.InvokeAsync("Register", identity);
            Log.Information($"Registered as {identity}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to register after reconnecting");
        }
    }

    static async Task ReconnectLoopAsync(HubConnection connection, string identity)
    {
        int retryDelay = 1000; // start with 1 second
        while (true)
        {
            try
            {
                Log.Information("Attempting to connect to server...");
                await connection.StartAsync();
                await Register(connection, identity);
                break; // success
            }
            catch (Exception ex)
            {
                Log.Error($"Connection failed: {ex.Message}. Retrying in {retryDelay / 1000} seconds...");
                await Task.Delay(retryDelay);
                retryDelay = Math.Min(retryDelay * 2, 30000); // exponential up to 30 sec
            }
        }
    }

    private static Result TryLaunchNotepad(string parameter)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "notepad.exe",
                    Arguments = $"{parameter}",
                    UseShellExecute = false,
                    CreateNoWindow = false,
                },
            };

            process.Start();

            Log.Information("Notepad launched successfully.");
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
