using System.ComponentModel.DataAnnotations;

namespace ShopEase.Models;

/// <summary>
/// Ligne du panier. Le panier est identifié par <see cref="CartId"/> :
/// un identifiant anonyme (cookie) pour un visiteur, ou l'Id de l'utilisateur une fois connecté.
/// </summary>
public class CartItem
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string CartId { get; set; } = string.Empty;

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    [Range(1, 99)]
    public int Quantity { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
