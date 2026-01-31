using BookStore.Application.Interfaces;
using BookStore.Application.Options;
using BookStore.Infrastructure.Data;
using BookStore.Infrastructure.Repositories;
using BookStore.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookStore.Infrastructure;

/// <summary>
/// Dependency injection extensions for the Infrastructure layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Infrastructure layer services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration,
        string? databasePath = null)
    {
        var connectionString = databasePath != null
            ? $"Data Source={databasePath}"
            : configuration.GetConnectionString("DefaultConnection") ?? "Data Source=BookStore.db";
        
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<IBookService, BookService>();
        services.AddHttpClient<IBookFetchService, BookFetchService>();
        services.Configure<BookFetchSettings>(
            configuration.GetSection(BookFetchSettings.SectionName));

        return services;
    }
}
