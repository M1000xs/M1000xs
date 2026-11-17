using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopEase.Data;
using ShopEase.Models;

namespace ShopEase.Areas.Admin.Controllers;

/// <summary>Gestion du catalogue (CRUD) réservée aux administrateurs.</summary>
[Area("Admin"), Authorize(Roles = DbSeeder.AdminRole)]
public class ProductsController : Controller
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".svg" };
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public ProductsController(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var q = _db.Products.Include(p => p.Category).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(p => p.Name.ToLower().Contains(search.ToLower()));
        ViewBag.Search = search;
        return View(await q.OrderBy(p => p.Category!.Name).ThenBy(p => p.Name).ToListAsync());
    }

    public async Task<IActionResult> Create()
    {
        await LoadCategoriesAsync();
        return View("Edit", new Product());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product, IFormFile? image)
    {
        await HandleUploadAsync(product, image);
        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync();
            return View("Edit", product);
        }
        product.CreatedAt = DateTime.UtcNow;
        product.Images.Add(new ProductImage { Url = product.ImageUrl });
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Produit « {product.Name} » créé.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null) return NotFound();
        await LoadCategoriesAsync();
        return View(product);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product input, IFormFile? image)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null) return NotFound();

        await HandleUploadAsync(input, image);
        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync();
            input.Id = id;
            return View(input);
        }

        product.Name = input.Name;
        product.Description = input.Description;
        product.Price = input.Price;
        product.Stock = input.Stock;
        product.CategoryId = input.CategoryId;
        product.Rating = input.Rating;
        if (product.ImageUrl != input.ImageUrl)
        {
            product.ImageUrl = input.ImageUrl;
            _db.ProductImages.Add(new ProductImage { ProductId = id, Url = input.ImageUrl });
        }
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Produit « {product.Name} » mis à jour.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null) return NotFound();

        if (await _db.OrderItems.AnyAsync(i => i.ProductId == id))
        {
            TempData["Error"] = "Ce produit figure dans des commandes : mettez son stock à 0 plutôt que de le supprimer.";
            return RedirectToAction(nameof(Index));
        }
        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Produit « {product.Name} » supprimé.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadCategoriesAsync() =>
        ViewBag.Categories = new SelectList(await _db.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");

    /// <summary>Enregistre l'image envoyée dans wwwroot/images/uploads et met à jour ImageUrl.</summary>
    private async Task HandleUploadAsync(Product product, IFormFile? image)
    {
        if (image is null || image.Length == 0)
        {
            if (string.IsNullOrWhiteSpace(product.ImageUrl))
                product.ImageUrl = "/images/products/placeholder.svg";
            ModelState.Remove(nameof(Product.ImageUrl));
            return;
        }

        var ext = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext) || image.Length > 2 * 1024 * 1024)
        {
            ModelState.AddModelError(nameof(Product.ImageUrl), "Image invalide (jpg, png, webp, svg ; 2 Mo max).");
            return;
        }

        var dir = Path.Combine(_env.WebRootPath, "images", "uploads");
        Directory.CreateDirectory(dir);
        var fileName = $"{Guid.NewGuid():N}{ext}";
        await using (var stream = System.IO.File.Create(Path.Combine(dir, fileName)))
            await image.CopyToAsync(stream);
        product.ImageUrl = $"/images/uploads/{fileName}";
        ModelState.Remove(nameof(Product.ImageUrl));
    }
}
