using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopEase.Models;

public enum OrderStatus
{
    [Display(Name = "En attente")] Pending,
    [Display(Name = "Payée")] Paid,
    [Display(Name = "Expédiée")] Shipped,
    [Display(Name = "Livrée")] Delivered,
    [Display(Name = "Annulée")] Cancelled
}

public enum PaymentMethod
{
    [Display(Name = "Carte bancaire")] CreditCard,
    [Display(Name = "Paiement à la livraison")] CashOnDelivery
}

/// <summary>Commande passée par un utilisateur, avec ses informations d'expédition.</summary>
public class Order
{
    public int Id { get; set; }

    /// <summary>Numéro lisible généré à la validation (ex. CMD-20261117-4F2A9C).</summary>
    [Required, StringLength(30)]
    public string OrderNumber { get; set; } = string.Empty;

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public PaymentMethod PaymentMethod { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal SubTotal { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal ShippingCost { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Total { get; set; }

    // --- Informations d'expédition ---
    [Required, StringLength(120)] public string ShippingFullName { get; set; } = string.Empty;
    [Required, StringLength(200)] public string ShippingAddress { get; set; } = string.Empty;
    [Required, StringLength(80)] public string ShippingCity { get; set; } = string.Empty;
    [Required, StringLength(10)] public string ShippingPostalCode { get; set; } = string.Empty;
    [Required, StringLength(60)] public string ShippingCountry { get; set; } = "Tunisie";
    [Required, StringLength(20)] public string ShippingPhone { get; set; } = string.Empty;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
