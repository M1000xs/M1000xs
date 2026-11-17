using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShopEase.Models;

namespace ShopEase.Data;

/// <summary>Applique les migrations puis crée les rôles, comptes de démo, catégories, produits et commandes d'exemple.</summary>
public static class DbSeeder
{
    public const string AdminRole = "Admin";
    public const string CustomerRole = "Client";

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await db.Database.MigrateAsync();

        foreach (var role in new[] { AdminRole, CustomerRole })
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        var admin = await EnsureUserAsync(userManager, "admin@shopease.tn", "Admin123", "Admin", "ShopEase", AdminRole);
        var client = await EnsureUserAsync(userManager, "client@shopease.tn", "Client123", "Ali", "Ben Salah", CustomerRole,
            address: "12 rue de la République", city: "Gafsa", postal: "2100", phone: "+216 22 123 456");

        if (!await db.Categories.AnyAsync())
        {
            var categories = SeedCatalog.Categories
                .Select(c => new Category { Name = c.Name, Description = c.Description }).ToList();
            db.Categories.AddRange(categories);
            await db.SaveChangesAsync();

            var byName = categories.ToDictionary(c => c.Name);
            foreach (var p in SeedCatalog.Products)
            {
                var product = new Product
                {
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    SalesCount = p.Sales,
                    Rating = p.Rating,
                    CreatedAt = DateTime.UtcNow.AddDays(-p.DaysAgo),
                    CategoryId = byName[p.Category].Id,
                    ImageUrl = $"/images/products/{p.Slug}-1.svg"
                };
                for (var i = 1; i <= 3; i++)
                    product.Images.Add(new ProductImage { Url = $"/images/products/{p.Slug}-{i}.svg" });
                db.Products.Add(product);
            }
            await db.SaveChangesAsync();
        }

        // Deux commandes d'exemple pour remplir l'historique du compte client.
        if (!await db.Orders.AnyAsync())
        {
            var products = await db.Products.OrderBy(p => p.Id).ToListAsync();
            db.Orders.Add(BuildOrder(client, DateTime.UtcNow.AddDays(-20), OrderStatus.Delivered, PaymentMethod.CreditCard,
                (products[2], 1), (products[14], 2)));
            db.Orders.Add(BuildOrder(client, DateTime.UtcNow.AddDays(-4), OrderStatus.Shipped, PaymentMethod.CashOnDelivery,
                (products[6], 1), (products[9], 1)));
            await db.SaveChangesAsync();
        }
    }

    private static Order BuildOrder(ApplicationUser user, DateTime date, OrderStatus status, PaymentMethod pay,
        params (Product product, int qty)[] lines)
    {
        var order = new Order
        {
            OrderNumber = $"CMD-{date:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}",
            UserId = user.Id,
            OrderDate = date,
            Status = status,
            PaymentMethod = pay,
            ShippingFullName = user.FullName,
            ShippingAddress = user.Address ?? "",
            ShippingCity = user.City ?? "",
            ShippingPostalCode = user.PostalCode ?? "",
            ShippingPhone = user.PhoneNumber ?? ""
        };
        foreach (var (p, q) in lines)
            order.Items.Add(new OrderItem { ProductId = p.Id, ProductName = p.Name, UnitPrice = p.Price, Quantity = q });
        order.SubTotal = order.Items.Sum(i => i.UnitPrice * i.Quantity);
        order.ShippingCost = ViewModels.ShippingRules.Cost(order.SubTotal);
        order.Total = order.SubTotal + order.ShippingCost;
        return order;
    }

    private static async Task<ApplicationUser> EnsureUserAsync(UserManager<ApplicationUser> um, string email, string password,
        string first, string last, string role, string? address = null, string? city = null, string? postal = null, string? phone = null)
    {
        var user = await um.FindByEmailAsync(email);
        if (user is not null) return user;

        user = new ApplicationUser
        {
            UserName = email, Email = email, EmailConfirmed = true,
            FirstName = first, LastName = last,
            Address = address, City = city, PostalCode = postal, PhoneNumber = phone
        };
        var result = await um.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        await um.AddToRoleAsync(user, role);
        return user;
    }
}
