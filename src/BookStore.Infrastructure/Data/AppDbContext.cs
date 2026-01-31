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
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderBook> OrderBooks => Set<OrderBook>();

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

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Address)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.CreationDate)
                .IsRequired();

            entity.Property(e => e.TotalCost)
                .HasPrecision(18, 2);
        });

        modelBuilder.Entity<OrderBook>(entity =>
        {
            entity.HasKey(e => new { e.OrderId, e.BookId });

            entity.Property(e => e.Quantity)
                .IsRequired();

            entity.Property(e => e.PriceAtPurchase)
                .HasPrecision(18, 2);

            entity.HasOne(e => e.Order)
                .WithMany(o => o.OrderBooks)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Book)
                .WithMany(b => b.OrderBooks)
                .HasForeignKey(e => e.BookId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
