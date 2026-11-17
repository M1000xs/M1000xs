using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopEase.Data;
using ShopEase.Models;
using ShopEase.Services;
using ShopEase.ViewModels;

namespace ShopEase.Controllers;

/// <summary>Catalogue : liste paginée avec filtres + page de détails.</summary>
public class ProductsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ICartService _cart;

    public ProductsController(ApplicationDbContext db, ICartService cart)
    {
        _db = db;
        _cart = cart;
    }

    // GET /Products?search=&categoryId=&minPrice=&maxPrice=&inStockOnly=&sort=&page=
    public async Task<IActionResult> Index([FromQuery] ProductFilter filter)
    {
        filter.Page = Math.Max(1, filter.Page);
        filter.PageSize = filter.PageSize is < 3 or > 48 ? 9 : filter.PageSize;

        IQueryable<Product> query = _db.Products.AsNoTracking().Include(p => p.Category);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term) || p.Description.ToLower().Contains(term));
        }
        if (filter.CategoryId is int catId)
            query = query.Where(p => p.CategoryId == catId);
        if (filter.MinPrice is decimal min)
            query = query.Where(p => p.Price >= min);
        if (filter.MaxPrice is decimal max)
            query = query.Where(p => p.Price <= max);
        if (filter.InStockOnly)
            query = query.Where(p => p.Stock > 0);

        query = filter.Sort switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            "rating" => query.OrderByDescending(p => p.Rating).ThenByDescending(p => p.SalesCount),
            "name" => query.OrderBy(p => p.Name),
            _ => query.OrderByDescending(p => p.SalesCount) // popularité
        };

        var total = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)filter.PageSize));
        if (filter.Page > totalPages) filter.Page = totalPages;

        var products = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var vm = new ProductListViewModel
        {
            Products = products,
            Categories = await _db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(),
            Filter = filter,
            TotalCount = total
        };
        return View(vm);
    }

    // GET /Products/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var product = await _db.Products.AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();

        var related = await _db.Products.AsNoTracking()
            .Where(p => p.CategoryId == product.CategoryId && p.Id != id)
            .OrderByDescending(p => p.SalesCount).Take(4).ToListAsync();

        var cart = await _cart.GetCartAsync();
        return View(new ProductDetailsViewModel
        {
            Product = product,
            Related = related,
            QuantityInCart = cart.Items.FirstOrDefault(i => i.ProductId == id)?.Quantity ?? 0
        });
    }
}
