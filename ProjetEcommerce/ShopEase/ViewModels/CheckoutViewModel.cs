using System.ComponentModel.DataAnnotations;
using ShopEase.Models;

namespace ShopEase.ViewModels;

public class CheckoutViewModel
{
    [Required(ErrorMessage = "Le nom complet est obligatoire.")]
    [StringLength(120), Display(Name = "Nom complet")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'adresse est obligatoire.")]
    [StringLength(200), Display(Name = "Adresse")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "La ville est obligatoire.")]
    [StringLength(80), Display(Name = "Ville")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le code postal est obligatoire.")]
    [RegularExpression(@"^\d{4,5}$", ErrorMessage = "Code postal invalide (4 ou 5 chiffres).")]
    [Display(Name = "Code postal")]
    public string PostalCode { get; set; } = string.Empty;

    [Required, StringLength(60), Display(Name = "Pays")]
    public string Country { get; set; } = "Tunisie";

    [Required(ErrorMessage = "Le téléphone est obligatoire.")]
    [RegularExpression(@"^\+?[\d\s]{8,15}$", ErrorMessage = "Numéro de téléphone invalide.")]
    [Display(Name = "Téléphone")]
    public string Phone { get; set; } = string.Empty;

    [Display(Name = "Mode de paiement")]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CreditCard;

    // Champs carte (simulation : aucun paiement réel, rien n'est stocké)
    [Display(Name = "Numéro de carte")]
    public string? CardNumber { get; set; }

    [Display(Name = "Expiration (MM/AA)")]
    public string? CardExpiry { get; set; }

    [Display(Name = "CVV")]
    public string? CardCvv { get; set; }

    [Display(Name = "Enregistrer cette adresse dans mon profil")]
    public bool SaveAddress { get; set; } = true;

    public CartViewModel Cart { get; set; } = new();
}
