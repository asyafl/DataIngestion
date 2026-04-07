using DataIngestion.Api.Middleware;
using DataIngestion.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Host.UseSerilog((context, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day);
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

var app = builder.Build();

await ApplyMigrationsAsync(app);

app.UseMiddleware<GlobalExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

app.MapControllers();

app.Run();


static async Task ApplyMigrationsAsync(WebApplication app)
{
    const int maxRetries = 10;
    var delay = TimeSpan.FromSeconds(5);

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    for (var attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            await dbContext.Database.MigrateAsync();
            Log.Information("Database migrations applied successfully");
            return;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to apply database migrations on attempt {Attempt}/{MaxRetries}", attempt, maxRetries);

            if (attempt == maxRetries)
                throw;

            await Task.Delay(delay);
        }
    }
}
