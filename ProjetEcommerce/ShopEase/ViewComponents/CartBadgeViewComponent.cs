using Microsoft.AspNetCore.Mvc;
using ShopEase.Services;

namespace ShopEase.ViewComponents;

/// <summary>Affiche le nombre d'articles du panier dans la barre de navigation.</summary>
public class CartBadgeViewComponent : ViewComponent
{
    private readonly ICartService _cart;
    public CartBadgeViewComponent(ICartService cart) => _cart = cart;

    public async Task<IViewComponentResult> InvokeAsync() => View(await _cart.GetCountAsync());
}
