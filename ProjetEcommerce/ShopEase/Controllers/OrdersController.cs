using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopEase.Models;
using ShopEase.Services;
using ShopEase.ViewModels;

namespace ShopEase.Controllers;

/// <summary>Validation de commande (expédition + paiement simulé), confirmation et détail d'une commande.</summary>
[Authorize]
public class OrdersController : Controller
{
    private readonly ICartService _cart;
    private readonly IOrderService _orders;
    private readonly UserManager<ApplicationUser> _users;

    public OrdersController(ICartService cart, IOrderService orders, UserManager<ApplicationUser> users)
    {
        _cart = cart;
        _orders = orders;
        _users = users;
    }

    // GET /Orders/Checkout
    public async Task<IActionResult> Checkout()
    {
        var cart = await _cart.GetCartAsync();
        if (cart.Items.Count == 0)
        {
            TempData["Error"] = "Votre panier est vide.";
            return RedirectToAction("Index", "Cart");
        }

        var user = (await _users.GetUserAsync(User))!;
        return View(new CheckoutViewModel
        {
            FullName = user.FullName,
            Address = user.Address ?? "",
            City = user.City ?? "",
            PostalCode = user.PostalCode ?? "",
            Phone = user.PhoneNumber ?? "",
            Cart = cart
        });
    }

    // POST /Orders/Checkout
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model)
    {
        if (model.PaymentMethod == PaymentMethod.CreditCard)
            ValidateCard(model);

        if (!ModelState.IsValid)
        {
            model.Cart = await _cart.GetCartAsync();
            return View(model);
        }

        var user = (await _users.GetUserAsync(User))!;
        var (order, error) = await _orders.PlaceOrderAsync(user.Id, model);
        if (order is null)
        {
            TempData["Error"] = error;
            return RedirectToAction("Index", "Cart");
        }

        if (model.SaveAddress)
        {
            user.Address = model.Address;
            user.City = model.City;
            user.PostalCode = model.PostalCode;
            user.PhoneNumber = model.Phone;
            await _users.UpdateAsync(user);
        }

        return RedirectToAction(nameof(Confirmation), new { id = order.OrderNumber });
    }

    // GET /Orders/Confirmation/CMD-...
    public async Task<IActionResult> Confirmation(string id)
    {
        var order = await _orders.GetUserOrderAsync(_users.GetUserId(User)!, id);
        return order is null ? NotFound() : View(order);
    }

    // GET /Orders/Details/CMD-...
    public async Task<IActionResult> Details(string id)
    {
        var order = await _orders.GetUserOrderAsync(_users.GetUserId(User)!, id);
        return order is null ? NotFound() : View(order);
    }

    /// <summary>Contrôles de format uniquement : le paiement est simulé, la carte n'est jamais enregistrée.</summary>
    private void ValidateCard(CheckoutViewModel m)
    {
        var digits = new string((m.CardNumber ?? "").Where(char.IsDigit).ToArray());
        if (digits.Length != 16 || !LuhnValid(digits))
            ModelState.AddModelError(nameof(m.CardNumber), "Numéro de carte invalide (16 chiffres).");

        if (!System.Text.RegularExpressions.Regex.IsMatch(m.CardExpiry ?? "", @"^(0[1-9]|1[0-2])/\d{2}$"))
            ModelState.AddModelError(nameof(m.CardExpiry), "Format attendu : MM/AA.");
        else
        {
            var month = int.Parse(m.CardExpiry![..2]);
            var year = 2000 + int.Parse(m.CardExpiry[3..]);
            if (new DateTime(year, month, 1).AddMonths(1) <= DateTime.UtcNow)
                ModelState.AddModelError(nameof(m.CardExpiry), "La carte est expirée.");
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(m.CardCvv ?? "", @"^\d{3}$"))
            ModelState.AddModelError(nameof(m.CardCvv), "CVV invalide (3 chiffres).");
    }

    private static bool LuhnValid(string digits)
    {
        var sum = 0;
        for (var i = 0; i < digits.Length; i++)
        {
            var d = digits[digits.Length - 1 - i] - '0';
            if (i % 2 == 1) { d *= 2; if (d > 9) d -= 9; }
            sum += d;
        }
        return sum % 10 == 0;
    }
}
