
using Infrastructure;
using WorkerService;

HostApplicationBuilder? builder = Host.CreateApplicationBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHostedService<DbMaintenanceWorker>();

IHost? host = builder.Build();
host.Run();
