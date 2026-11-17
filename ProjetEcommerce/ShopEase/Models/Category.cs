using System.ComponentModel.DataAnnotations;

namespace ShopEase.Models;

/// <summary>Catégorie de produits (ex. : Informatique, Maison...).</summary>
public class Category
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    [Display(Name = "Nom")]
    public string Name { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Description { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
