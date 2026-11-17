using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopEase.Data;
using ShopEase.ViewModels;

namespace ShopEase.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;

    public HomeController(ApplicationDbContext db) => _db = db;

    /// <summary>Page d'accueil : catégories, produits populaires et nouveautés.</summary>
    public async Task<IActionResult> Index()
    {
        ViewBag.Categories = await _db.Categories.Include(c => c.Products).OrderBy(c => c.Name).ToListAsync();
        ViewBag.Popular = await _db.Products.Include(p => p.Category)
            .OrderByDescending(p => p.SalesCount).Take(4).ToListAsync();
        ViewBag.Newest = await _db.Products.Include(p => p.Category)
            .OrderByDescending(p => p.CreatedAt).Take(4).ToListAsync();
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
