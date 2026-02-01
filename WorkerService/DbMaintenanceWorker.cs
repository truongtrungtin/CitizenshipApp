using Infrastructure.Persistence;

namespace WorkerService;

public sealed class DbMaintenanceWorker : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<DbMaintenanceWorker> _logger;

    public DbMaintenanceWorker(IServiceProvider sp, ILogger<DbMaintenanceWorker> logger)
    {
        _sp = sp;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Run once on startup
        await RunOnce(stoppingToken);

        // Run daily
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            await RunOnce(stoppingToken);
        }
    }

    private async Task RunOnce(CancellationToken ct)
    {
        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        _logger.LogInformation("Worker maintenance started.");

        await SeedData.SeedAsync(db);

        // Optional cleanup example:
        // int retentionDays = 180;
        // var cutoff = DateTime.UtcNow.AddDays(-retentionDays);
        // await db.StudyEvents.Where(x => x.CreatedUtc < cutoff).ExecuteDeleteAsync(ct);

        _logger.LogInformation("Worker maintenance finished.");
    }
}
