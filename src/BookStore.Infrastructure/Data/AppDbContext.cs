using BookStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Infrastructure.Data;

/// <summary>
/// Entity Framework Core database context for the BookStore application.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Number)
                .IsRequired();

            entity.HasIndex(e => e.Number)
                .IsUnique();

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.OriginalTitle)
                .HasMaxLength(500);

            entity.Property(e => e.Description)
                .HasMaxLength(4000);

            entity.Property(e => e.Cover)
                .HasMaxLength(1000);

            entity.Property(e => e.Price)
                .HasPrecision(18, 2);
        });
    }
}
