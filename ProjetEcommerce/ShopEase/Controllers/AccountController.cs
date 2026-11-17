using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopEase.Data;
using ShopEase.Models;
using ShopEase.Services;
using ShopEase.ViewModels;

namespace ShopEase.Controllers;

/// <summary>Inscription, connexion, déconnexion et profil utilisateur (ASP.NET Core Identity).</summary>
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly ICartService _cart;
    private readonly IOrderService _orders;

    public AccountController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn,
        ICartService cart, IOrderService orders)
    {
        _users = users;
        _signIn = signIn;
        _cart = cart;
        _orders = orders;
    }

    // ---------- Inscription ----------
    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(model);

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName
        };
        var result = await _users.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, Translate(e));
            return View(model);
        }

        await _users.AddToRoleAsync(user, DbSeeder.CustomerRole);
        await _cart.MergeAnonymousCartAsync(user.Id);
        await _signIn.SignInAsync(user, isPersistent: false);
        TempData["Success"] = $"Bienvenue {user.FirstName} ! Votre compte a été créé.";
        return LocalRedirectOrHome(returnUrl);
    }

    // ---------- Connexion ----------
    [HttpGet]
    public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _signIn.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Compte temporairement verrouillé après plusieurs échecs. Réessayez plus tard.");
            return View(model);
        }
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Email ou mot de passe incorrect.");
            return View(model);
        }

        var user = await _users.FindByEmailAsync(model.Email);
        await _cart.MergeAnonymousCartAsync(user!.Id);
        TempData["Success"] = $"Bonjour {user.FirstName} !";
        return LocalRedirectOrHome(model.ReturnUrl);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        TempData["Success"] = "Vous êtes déconnecté.";
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied() => View();

    // ---------- Profil ----------
    [Authorize, HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = (await _users.GetUserAsync(User))!;
        return View(await BuildProfileAsync(user));
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        var user = (await _users.GetUserAsync(User))!;
        if (!ModelState.IsValid)
        {
            model.Email = user.Email;
            model.RegisteredAt = user.RegisteredAt;
            model.Orders = await _orders.GetUserOrdersAsync(user.Id);
            return View(model);
        }

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.PhoneNumber = model.PhoneNumber;
        user.Address = model.Address;
        user.City = model.City;
        user.PostalCode = model.PostalCode;
        await _users.UpdateAsync(user);

        TempData["Success"] = "Vos informations ont été mises à jour.";
        return RedirectToAction(nameof(Profile));
    }

    [Authorize, HttpGet]
    public IActionResult ChangePassword() => View();

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = (await _users.GetUserAsync(User))!;
        var result = await _users.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, Translate(e));
            return View(model);
        }
        await _signIn.RefreshSignInAsync(user);
        TempData["Success"] = "Mot de passe modifié.";
        return RedirectToAction(nameof(Profile));
    }

    // ---------- Utilitaires ----------
    private async Task<ProfileViewModel> BuildProfileAsync(ApplicationUser user) => new()
    {
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email,
        PhoneNumber = user.PhoneNumber,
        Address = user.Address,
        City = user.City,
        PostalCode = user.PostalCode,
        RegisteredAt = user.RegisteredAt,
        Orders = await _orders.GetUserOrdersAsync(user.Id)
    };

    private IActionResult LocalRedirectOrHome(string? returnUrl) =>
        Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : RedirectToAction("Index", "Home");

    private static string Translate(IdentityError e) => e.Code switch
    {
        "DuplicateUserName" or "DuplicateEmail" => "Un compte existe déjà avec cet email.",
        "PasswordTooShort" => "Le mot de passe est trop court (6 caractères minimum).",
        "PasswordRequiresDigit" => "Le mot de passe doit contenir au moins un chiffre.",
        "PasswordRequiresLower" => "Le mot de passe doit contenir au moins une minuscule.",
        "PasswordMismatch" => "Mot de passe actuel incorrect.",
        _ => e.Description
    };
}
