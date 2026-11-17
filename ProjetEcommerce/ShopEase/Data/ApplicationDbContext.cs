using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShopEase.Models;

namespace ShopEase.Data;

/// <summary>Contexte EF Core : tables métier + tables ASP.NET Core Identity.</summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Category>().HasIndex(c => c.Name).IsUnique();

        builder.Entity<Product>(e =>
        {
            e.HasOne(p => p.Category).WithMany(c => c.Products)
             .HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(p => p.Name);
        });

        builder.Entity<ProductImage>()
            .HasOne(i => i.Product).WithMany(p => p.Images)
            .HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CartItem>(e =>
        {
            e.HasIndex(c => new { c.CartId, c.ProductId }).IsUnique();
            e.HasOne(c => c.Product).WithMany()
             .HasForeignKey(c => c.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Order>(e =>
        {
            e.HasIndex(o => o.OrderNumber).IsUnique();
            e.HasOne(o => o.User).WithMany(u => u.Orders)
             .HasForeignKey(o => o.UserId).OnDelete(DeleteBehavior.Restrict);
            e.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(o => o.PaymentMethod).HasConversion<string>().HasMaxLength(20);
        });

        builder.Entity<OrderItem>(e =>
        {
            e.HasOne(i => i.Order).WithMany(o => o.Items)
             .HasForeignKey(i => i.OrderId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(i => i.Product).WithMany()
             .HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        // SQLite ne sait ni trier ni sommer des colonnes "decimal" :
        // on les stocke donc en REAL (double) quand ce fournisseur est utilisé.
        if (Database.IsSqlite())
        {
            foreach (var entity in builder.Model.GetEntityTypes())
            foreach (var prop in entity.GetProperties()
                         .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                prop.SetProviderClrType(typeof(double));
                prop.SetColumnType("REAL");
            }
        }
    }
}
