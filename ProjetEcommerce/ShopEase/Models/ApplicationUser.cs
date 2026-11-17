using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ShopEase.Models;

/// <summary>Utilisateur du site : étend IdentityUser (email, mot de passe haché, téléphone...).</summary>
public class ApplicationUser : IdentityUser
{
    [StringLength(60)]
    public string FirstName { get; set; } = string.Empty;

    [StringLength(60)]
    public string LastName { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Address { get; set; }

    [StringLength(80)]
    public string? City { get; set; }

    [StringLength(10)]
    public string? PostalCode { get; set; }

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    public ICollection<Order> Orders { get; set; } = new List<Order>();

    public string FullName => $"{FirstName} {LastName}".Trim();
}
