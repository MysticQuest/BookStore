using BookStore.Api.Hubs;
using BookStore.Api.Jobs;
using BookStore.Api.Middleware;
using BookStore.Application.Interfaces;
using BookStore.Application.Options;
using BookStore.Infrastructure;
using BookStore.Infrastructure.Data;
using Hangfire;
using Hangfire.InMemory;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

var dbFolder = Environment.GetEnvironmentVariable("DB_PATH") 
    ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BookStore");
Directory.CreateDirectory(dbFolder);
var dbPath = Path.Combine(dbFolder, "BookStore.db");

Console.WriteLine($"Database location: {dbPath}");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration, dbPath);

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseInMemoryStorage());

builder.Services.AddHangfireServer();
builder.Services.AddScoped<BookFetchJob>();

builder.Services.AddSignalR();
builder.Services.AddSingleton<IJobStatusService, JobStatusService>();
builder.Services.AddSingleton<IBookHubNotifier, BookHubNotifier>();

builder.Services.AddOptions<CorsSettings>()
    .Bind(builder.Configuration.GetSection(CorsSettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var corsSettings = builder.Configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins(corsSettings?.AllowedOrigins ?? [])
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database");

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
    
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (dbContext.Database.IsRelational())
    {
        await dbContext.Database.MigrateAsync();
    }
    else
    {
        await dbContext.Database.EnsureCreatedAsync();
    }
}

app.UseGlobalExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapHangfireDashboard("/hangfire");
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowBlazorClient");
app.UseRateLimiter();
app.MapControllers();
app.MapHub<BookHub>("/hubs/books");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

RecurringJob.AddOrUpdate<BookFetchJob>(
    "book-fetch-job",
    job => job.ExecuteAsync(CancellationToken.None),
    Cron.Minutely);

app.Run();

/// <summary>
/// Entry point class for the API application.
/// Made public partial for integration testing with WebApplicationFactory.
/// </summary>
public partial class Program { }
