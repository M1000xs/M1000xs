using System.ComponentModel.DataAnnotations;
using ShopEase.Models;

namespace ShopEase.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Le prénom est obligatoire."), StringLength(60), Display(Name = "Prénom")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nom est obligatoire."), StringLength(60), Display(Name = "Nom")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'email est obligatoire."), EmailAddress(ErrorMessage = "Email invalide.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Au moins 6 caractères.")]
    [DataType(DataType.Password), Display(Name = "Mot de passe")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password), Display(Name = "Confirmer le mot de passe")]
    [Compare(nameof(Password), ErrorMessage = "Les mots de passe ne correspondent pas.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class LoginViewModel
{
    [Required(ErrorMessage = "L'email est obligatoire."), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
    [DataType(DataType.Password), Display(Name = "Mot de passe")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Se souvenir de moi")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}

public class ProfileViewModel
{
    [Required(ErrorMessage = "Le prénom est obligatoire."), StringLength(60), Display(Name = "Prénom")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nom est obligatoire."), StringLength(60), Display(Name = "Nom")]
    public string LastName { get; set; } = string.Empty;

    [EmailAddress, Display(Name = "Email")]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Numéro invalide."), Display(Name = "Téléphone")]
    public string? PhoneNumber { get; set; }

    [StringLength(200), Display(Name = "Adresse")]
    public string? Address { get; set; }

    [StringLength(80), Display(Name = "Ville")]
    public string? City { get; set; }

    [StringLength(10), Display(Name = "Code postal")]
    public string? PostalCode { get; set; }

    public DateTime RegisteredAt { get; set; }
    public IReadOnlyList<Order> Orders { get; set; } = Array.Empty<Order>();
}

public class ChangePasswordViewModel
{
    [Required, DataType(DataType.Password), Display(Name = "Mot de passe actuel")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 6, ErrorMessage = "Au moins 6 caractères.")]
    [DataType(DataType.Password), Display(Name = "Nouveau mot de passe")]
    public string NewPassword { get; set; } = string.Empty;

    [DataType(DataType.Password), Display(Name = "Confirmer")]
    [Compare(nameof(NewPassword), ErrorMessage = "Les mots de passe ne correspondent pas.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
