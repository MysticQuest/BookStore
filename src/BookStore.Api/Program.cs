using BookStore.Api.Hubs;
using BookStore.Api.Jobs;
using BookStore.Application.Interfaces;
using BookStore.Application.Options;
using BookStore.Infrastructure;
using BookStore.Infrastructure.Data;
using Hangfire;
using Hangfire.InMemory;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var dbFolder = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "BookStore");
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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowBlazorClient");
app.MapControllers();
app.MapHub<BookHub>("/hubs/books");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHangfireDashboard("/hangfire");

RecurringJob.AddOrUpdate<BookFetchJob>(
    "book-fetch-job",
    job => job.ExecuteAsync(),
    Cron.Minutely);

app.Run();
