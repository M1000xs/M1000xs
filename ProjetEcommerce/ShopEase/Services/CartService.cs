using Microsoft.EntityFrameworkCore;
using ShopEase.Data;
using ShopEase.Models;
using ShopEase.ViewModels;

namespace ShopEase.Services;

public interface ICartService
{
    string GetCartId();
    Task<CartViewModel> GetCartAsync();
    Task<int> GetCountAsync();
    Task<(bool ok, string message)> AddAsync(int productId, int quantity);
    Task<(bool ok, string message)> UpdateAsync(int productId, int quantity);
    Task RemoveAsync(int productId);
    Task ClearAsync();
    Task MergeAnonymousCartAsync(string userId);
}

/// <summary>
/// Panier persistant en base. Un visiteur anonyme reçoit un cookie "ShopEase.CartId" ;
/// à la connexion, son panier est fusionné dans celui de son compte (CartId = UserId).
/// </summary>
public class CartService : ICartService
{
    public const string CookieName = "ShopEase.CartId";
    private const int MaxQuantity = 99;

    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _http;

    public CartService(ApplicationDbContext db, IHttpContextAccessor http)
    {
        _db = db;
        _http = http;
    }

    private HttpContext Ctx => _http.HttpContext ?? throw new InvalidOperationException("Pas de contexte HTTP.");

    public string GetCartId()
    {
        var user = Ctx.User;
        if (user.Identity?.IsAuthenticated == true)
            return user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

        if (Ctx.Request.Cookies.TryGetValue(CookieName, out var id) && !string.IsNullOrEmpty(id))
            return id;

        // Le cookie a peut-être déjà été créé pendant cette requête.
        if (Ctx.Items[CookieName] is string pending) return pending;

        var newId = "anon-" + Guid.NewGuid().ToString("N");
        Ctx.Items[CookieName] = newId;
        Ctx.Response.Cookies.Append(CookieName, newId, new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        });
        return newId;
    }

    public async Task<CartViewModel> GetCartAsync()
    {
        var cartId = GetCartId();
        var items = await _db.CartItems
            .Include(c => c.Product)
            .Where(c => c.CartId == cartId)
            .OrderBy(c => c.AddedAt)
            .ToListAsync();
        return new CartViewModel { Items = items };
    }

    public async Task<int> GetCountAsync()
    {
        var cartId = GetCartId();
        return await _db.CartItems.Where(c => c.CartId == cartId).SumAsync(c => (int?)c.Quantity) ?? 0;
    }

    public async Task<(bool ok, string message)> AddAsync(int productId, int quantity)
    {
        if (quantity < 1) quantity = 1;
        var product = await _db.Products.FindAsync(productId);
        if (product is null) return (false, "Produit introuvable.");
        if (product.Stock == 0) return (false, $"« {product.Name} » est en rupture de stock.");

        var cartId = GetCartId();
        var item = await _db.CartItems.FirstOrDefaultAsync(c => c.CartId == cartId && c.ProductId == productId);
        var wanted = (item?.Quantity ?? 0) + quantity;
        var capped = Math.Min(wanted, Math.Min(product.Stock, MaxQuantity));

        if (item is null)
            _db.CartItems.Add(new CartItem { CartId = cartId, ProductId = productId, Quantity = capped });
        else
            item.Quantity = capped;

        await _db.SaveChangesAsync();
        return capped < wanted
            ? (true, $"Quantité limitée à {capped} (stock disponible).")
            : (true, $"« {product.Name} » a été ajouté au panier.");
    }

    public async Task<(bool ok, string message)> UpdateAsync(int productId, int quantity)
    {
        var cartId = GetCartId();
        var item = await _db.CartItems.Include(c => c.Product)
            .FirstOrDefaultAsync(c => c.CartId == cartId && c.ProductId == productId);
        if (item is null) return (false, "Article absent du panier.");

        if (quantity <= 0)
        {
            _db.CartItems.Remove(item);
            await _db.SaveChangesAsync();
            return (true, "Article retiré du panier.");
        }

        var max = Math.Min(item.Product!.Stock, MaxQuantity);
        item.Quantity = Math.Min(quantity, max);
        await _db.SaveChangesAsync();
        return item.Quantity < quantity
            ? (true, $"Quantité limitée à {item.Quantity} (stock disponible).")
            : (true, "Quantité mise à jour.");
    }

    public async Task RemoveAsync(int productId)
    {
        var cartId = GetCartId();
        await _db.CartItems.Where(c => c.CartId == cartId && c.ProductId == productId).ExecuteDeleteAsync();
    }

    public async Task ClearAsync()
    {
        var cartId = GetCartId();
        await _db.CartItems.Where(c => c.CartId == cartId).ExecuteDeleteAsync();
    }

    public async Task MergeAnonymousCartAsync(string userId)
    {
        if (!Ctx.Request.Cookies.TryGetValue(CookieName, out var anonId) || string.IsNullOrEmpty(anonId))
            return;

        var anonItems = await _db.CartItems.Include(c => c.Product).Where(c => c.CartId == anonId).ToListAsync();
        foreach (var a in anonItems)
        {
            var existing = await _db.CartItems.FirstOrDefaultAsync(c => c.CartId == userId && c.ProductId == a.ProductId);
            var max = Math.Min(a.Product!.Stock, MaxQuantity);
            if (existing is null)
                _db.CartItems.Add(new CartItem { CartId = userId, ProductId = a.ProductId, Quantity = Math.Min(a.Quantity, max) });
            else
                existing.Quantity = Math.Min(existing.Quantity + a.Quantity, max);
            _db.CartItems.Remove(a);
        }
        await _db.SaveChangesAsync();
        Ctx.Response.Cookies.Delete(CookieName);
    }
}
