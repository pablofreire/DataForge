using DataForge.BuildingBlocks.Infrastructure.DependencyInjection;
using DataForge.BuildingBlocks.Infrastructure.Observability;
using DataForge.Modules.Identity.Application.DependencyInjection;
using DataForge.Modules.Identity.Infrastructure.DependencyInjection;
using DataForge.Modules.Onboarding.Application.DependencyInjection;
using DataForge.Modules.Onboarding.Infrastructure.DependencyInjection;
using DataForge.Modules.Processing.Application.DependencyInjection;
using DataForge.Modules.Processing.Infrastructure.DependencyInjection;
using DataForge.Modules.Validation.Application.DependencyInjection;
using DataForge.Modules.Validation.Infrastructure.DependencyInjection;

// DataForge.Worker: runs background jobs (dataset import, validation) from the durable PostgreSQL queue.
// Scale out by running more instances; jobs are distributed with FOR UPDATE SKIP LOCKED.
var builder = Host.CreateApplicationBuilder(args);

builder.AddDataForgeObservability("dataforge-worker");

builder.Services
    .AddBuildingBlocksInfrastructure(builder.Configuration)
    .AddIdentityApplication()
    .AddIdentityInfrastructure()
    .AddOnboardingApplication()
    .AddOnboardingInfrastructure()
    .AddValidationApplication()
    .AddValidationInfrastructure()
    .AddProcessingApplication()
    .AddProcessingInfrastructure()
    .AddProcessingWorker();

// Give running jobs time to release their leases on shutdown (docker stop sends SIGTERM).
builder.Services.Configure<HostOptions>(options => options.ShutdownTimeout = TimeSpan.FromSeconds(30));

var host = builder.Build();
await host.RunAsync();
