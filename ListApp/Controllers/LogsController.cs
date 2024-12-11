using ListApp.Controllers;
using ListApp.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Serilog;

public class LogsController : Controller
{
    private readonly string _connectionString;
    private readonly Serilog.ILogger _logger;

    public LogsController(IConfiguration configuration, Serilog.ILogger logger)
    {
        _connectionString = configuration.GetConnectionString("LogsDatabase")
                            ?? throw new InvalidOperationException("Connection string 'LogsDatabase' is not configured.");
        _logger = logger.ForContext<LogsController>();
    }

    public async Task<IActionResult> List()
    {
        var logs = new List<Logs>();

        try
        {
            // Get today's date in the format SQLite uses for date filtering
            string todayDate = DateTime.Now.ToString("yyyy-MM-dd");

            // Open SQLite connection
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT TimeStamp, Level, RenderedMessage 
                FROM Logs 
                WHERE DATE(TimeStamp) = @TodayDate 
                ORDER BY TimeStamp DESC";
            command.Parameters.AddWithValue("@TodayDate", todayDate);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                logs.Add(new Logs
                {
                    TimeStamp = reader.GetDateTime(0),
                    Level = reader.GetString(1),
                    RenderedMessage = reader.GetString(2)
                });
            }

            // Log the number of retrieved records
            _logger.Information($"Retrieved {logs.Count} logs for {todayDate}.");
        }
        catch (Exception ex)
        {
            _logger.Error($"Error occurred while executing the SQL query: {ex.Message}");
            return View("Error");
        }

        return View(logs);
    }
}