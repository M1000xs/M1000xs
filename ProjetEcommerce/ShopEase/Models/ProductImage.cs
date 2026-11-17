using System.ComponentModel.DataAnnotations;

namespace ShopEase.Models;

/// <summary>Image supplémentaire d'un produit (galerie).</summary>
public class ProductImage
{
    public int Id { get; set; }

    [Required, StringLength(300)]
    public string Url { get; set; } = string.Empty;

    public int ProductId { get; set; }
    public Product? Product { get; set; }
}
