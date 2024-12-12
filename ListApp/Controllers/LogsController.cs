using ListApp.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using ListApp.Models;

public class LogsController : Controller
{
    private readonly string _connectionString;
    private readonly Serilog.ILogger _logger;
    private const int PageSize = 20;

    public LogsController(IConfiguration configuration, Serilog.ILogger logger)
    {
        _connectionString = configuration.GetConnectionString("LogsDatabase")
                            ?? throw new InvalidOperationException("Connection string 'LogsDatabase' is not configured.");
        _logger = logger.ForContext<LogsController>();
    }

    public async Task<IActionResult> List(int page = 1, string? level = null)
    {
        try
        {
            // Get today's date in the format SQLite uses for date filtering
            string todayDate = DateTime.Now.ToString("yyyy-MM-dd");

            // Prepare the log retrieval view model
            var viewModel = new LogPaginationViewModel
            {
                CurrentPage = page,
                PageSize = PageSize
            };

            // Open SQLite connection
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            // Prepare base query with optional level filtering
            string baseQuery = @"
                SELECT COUNT(*) OVER () AS TotalCount,
                       TimeStamp, 
                       Level, 
                       RenderedMessage 
                FROM Logs 
                WHERE DATE(TimeStamp) = @TodayDate 
                {0}
                ORDER BY TimeStamp DESC
                LIMIT @PageSize OFFSET @Offset";

            // Add level filtering if specified
            string levelFilter = string.IsNullOrEmpty(level) ? "" : "AND Level = @Level";
            baseQuery = string.Format(baseQuery, levelFilter);

            // Prepare and execute command
            using var command = connection.CreateCommand();
            command.CommandText = baseQuery;
            command.Parameters.AddWithValue("@TodayDate", todayDate);
            command.Parameters.AddWithValue("@PageSize", PageSize);
            command.Parameters.AddWithValue("@Offset", (page - 1) * PageSize);

            // Add level parameter if specified
            if (!string.IsNullOrEmpty(level))
            {
                command.Parameters.AddWithValue("@Level", level);
            }

            // Execute reader
            using var reader = await command.ExecuteReaderAsync();

            // Prepare logs list
            var logs = new List<Logs>();
            while (await reader.ReadAsync())
            {
                // First read sets the total count
                if (viewModel.TotalCount == 0)
                {
                    viewModel.TotalCount = reader.GetInt32(0);
                }

                logs.Add(new Logs
                {
                    TimeStamp = reader.GetDateTime(1),
                    Level = reader.GetString(2),
                    RenderedMessage = reader.GetString(3)
                });
            }

            viewModel.Logs = logs;
            viewModel.AvailableLevels = await GetAvailableLevelsAsync(connection);
            viewModel.SelectedLevel = level;

            // Calculate total pages
            viewModel.TotalPages = (int)Math.Ceiling((double)viewModel.TotalCount / PageSize);

            // Log the number of retrieved records
            _logger.Information($"Retrieved {viewModel.TotalCount} logs for {todayDate}, Page {page}, Level: {(level ?? "All Levels")}");

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.Error($"Error occurred while executing the SQL query: {ex.Message}");
            return View("Error");
        }
    }

    // Helper method to get available log levels
    private async Task<List<string>> GetAvailableLevelsAsync(SqliteConnection connection)
    {
        var levels = new List<string>();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT DISTINCT Level 
            FROM Logs 
            WHERE DATE(TimeStamp) = @TodayDate 
            ORDER BY Level";

        command.Parameters.AddWithValue("@TodayDate", DateTime.Now.ToString("yyyy-MM-dd"));

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            levels.Add(reader.GetString(0));
        }

        return levels;
    }
}

