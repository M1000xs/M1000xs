using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopEase.Models;

/// <summary>Produit vendu sur le site.</summary>
public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Le nom est obligatoire."), StringLength(120)]
    [Display(Name = "Nom")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "La description est obligatoire.")]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 100000, ErrorMessage = "Le prix doit être positif.")]
    [Column(TypeName = "decimal(10,2)")]
    [Display(Name = "Prix (DT)")]
    public decimal Price { get; set; }

    [Range(0, 100000)]
    [Display(Name = "Stock")]
    public int Stock { get; set; }

    /// <summary>Image principale affichée dans le catalogue.</summary>
    [StringLength(300)]
    [Display(Name = "Image principale")]
    public string ImageUrl { get; set; } = "/images/products/placeholder.svg";

    /// <summary>Nombre d'unités vendues : sert de critère de popularité.</summary>
    [Display(Name = "Ventes")]
    public int SalesCount { get; set; }

    /// <summary>Note moyenne sur 5.</summary>
    [Range(0, 5)]
    [Display(Name = "Note")]
    public double Rating { get; set; }

    [Display(Name = "Ajouté le")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Catégorie")]
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    /// <summary>Galerie d'images supplémentaires (page détails).</summary>
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
}
