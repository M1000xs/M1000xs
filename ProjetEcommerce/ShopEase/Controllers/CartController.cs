using Microsoft.AspNetCore.Mvc;
using ShopEase.Services;

namespace ShopEase.Controllers;

/// <summary>Panier : affichage, ajout, modification des quantités, suppression.</summary>
public class CartController : Controller
{
    private readonly ICartService _cart;

    public CartController(ICartService cart) => _cart = cart;

    public async Task<IActionResult> Index() => View(await _cart.GetCartAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity = 1, string? returnUrl = null)
    {
        var (ok, message) = await _cart.AddAsync(productId, quantity);
        TempData[ok ? "Success" : "Error"] = message;
        return Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int productId, int quantity)
    {
        var (ok, message) = await _cart.UpdateAsync(productId, quantity);
        TempData[ok ? "Success" : "Error"] = message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int productId)
    {
        await _cart.RemoveAsync(productId);
        TempData["Success"] = "Article retiré du panier.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear()
    {
        await _cart.ClearAsync();
        TempData["Success"] = "Le panier a été vidé.";
        return RedirectToAction(nameof(Index));
    }
}
