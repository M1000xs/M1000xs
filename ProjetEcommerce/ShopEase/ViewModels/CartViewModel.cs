using ShopEase.Models;

namespace ShopEase.ViewModels;

public class CartViewModel
{
    public IReadOnlyList<CartItem> Items { get; set; } = Array.Empty<CartItem>();
    public decimal SubTotal => Items.Sum(i => i.Product!.Price * i.Quantity);
    public decimal ShippingCost => ShippingRules.Cost(SubTotal);
    public decimal Total => SubTotal + ShippingCost;
    public int ItemCount => Items.Sum(i => i.Quantity);
}

/// <summary>Règle simple : livraison offerte à partir de 300 DT, sinon 8 DT.</summary>
public static class ShippingRules
{
    public const decimal FreeShippingThreshold = 300m;
    public const decimal StandardCost = 8m;
    public static decimal Cost(decimal subTotal) =>
        subTotal == 0 || subTotal >= FreeShippingThreshold ? 0m : StandardCost;
}
