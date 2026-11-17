using Microsoft.EntityFrameworkCore;
using ShopEase.Data;
using ShopEase.Models;
using ShopEase.ViewModels;

namespace ShopEase.Services;

public interface IOrderService
{
    Task<(Order? order, string? error)> PlaceOrderAsync(string userId, CheckoutViewModel model);
    Task<List<Order>> GetUserOrdersAsync(string userId);
    Task<Order?> GetUserOrderAsync(string userId, string orderNumber);
}

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _db;

    public OrderService(ApplicationDbContext db) => _db = db;

    /// <summary>Génère un numéro unique du type CMD-20261117-4F2A9C.</summary>
    public static string GenerateOrderNumber() =>
        $"CMD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";

    /// <summary>
    /// Transforme le panier en commande dans une transaction :
    /// vérification du stock, copie des lignes, décrément du stock, popularité, vidage du panier.
    /// </summary>
    public async Task<(Order? order, string? error)> PlaceOrderAsync(string userId, CheckoutViewModel model)
    {
        await using var tx = await _db.Database.BeginTransactionAsync();

        var cartItems = await _db.CartItems.Include(c => c.Product)
            .Where(c => c.CartId == userId).ToListAsync();
        if (cartItems.Count == 0) return (null, "Votre panier est vide.");

        foreach (var ci in cartItems)
            if (ci.Product!.Stock < ci.Quantity)
                return (null, $"Stock insuffisant pour « {ci.Product.Name} » (disponible : {ci.Product.Stock}).");

        var subTotal = cartItems.Sum(c => c.Product!.Price * c.Quantity);
        var shipping = ShippingRules.Cost(subTotal);

        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            PaymentMethod = model.PaymentMethod,
            // Paiement simulé : la carte est considérée comme acceptée.
            Status = model.PaymentMethod == PaymentMethod.CreditCard ? OrderStatus.Paid : OrderStatus.Pending,
            SubTotal = subTotal,
            ShippingCost = shipping,
            Total = subTotal + shipping,
            ShippingFullName = model.FullName,
            ShippingAddress = model.Address,
            ShippingCity = model.City,
            ShippingPostalCode = model.PostalCode,
            ShippingCountry = model.Country,
            ShippingPhone = model.Phone
        };

        foreach (var ci in cartItems)
        {
            order.Items.Add(new OrderItem
            {
                ProductId = ci.ProductId,
                ProductName = ci.Product!.Name,
                UnitPrice = ci.Product.Price,
                Quantity = ci.Quantity
            });
            ci.Product.Stock -= ci.Quantity;
            ci.Product.SalesCount += ci.Quantity;
        }

        _db.Orders.Add(order);
        _db.CartItems.RemoveRange(cartItems);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return (order, null);
    }

    public Task<List<Order>> GetUserOrdersAsync(string userId) =>
        _db.Orders.Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

    public Task<Order?> GetUserOrderAsync(string userId, string orderNumber) =>
        _db.Orders.Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.UserId == userId && o.OrderNumber == orderNumber);
}
