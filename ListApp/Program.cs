using ListApp.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Database setup
builder.Services.AddDbContext<ApplicationDbContext>(options => 
options.UseSqlite(builder.Configuration.GetConnectionString("ListApi")));

// Logging setup
var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.SQLite(
        Path.Combine(Environment.CurrentDirectory, "logs", "logs.db"), 
        batchSize: 5,
        retentionCheckInterval: TimeSpan.FromSeconds(5))
    .CreateLogger();

Log.Logger = logger; 
builder.Host.UseSerilog(logger);

// Add services
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<Serilog.ILogger>(logger);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Todos}/{action=List}/{id?}");

try
{
    Log.Information("Starting web application at {Time}", DateTime.Now);
    app.Run();
}
catch (Exception ex) when (ex is not OperationCanceledException)
{
    Log.Fatal(ex, "Application terminated unexpectedly at {Time}", DateTime.Now);
    throw;
}
finally
{
    Log.Information("Shutting down application...");
    Log.CloseAndFlush();
}
