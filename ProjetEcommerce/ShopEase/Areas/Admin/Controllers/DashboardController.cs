using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopEase.Data;
using ShopEase.Models;

namespace ShopEase.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = DbSeeder.AdminRole)]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _db;
    public DashboardController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        ViewBag.ProductCount = await _db.Products.CountAsync();
        ViewBag.OutOfStock = await _db.Products.CountAsync(p => p.Stock == 0);
        ViewBag.OrderCount = await _db.Orders.CountAsync();
        ViewBag.CustomerCount = await _db.Users.CountAsync();
        ViewBag.Revenue = await _db.Orders.Where(o => o.Status != OrderStatus.Cancelled).SumAsync(o => (decimal?)o.Total) ?? 0m;
        ViewBag.LatestOrders = await _db.Orders.Include(o => o.User)
            .OrderByDescending(o => o.OrderDate).Take(5).ToListAsync();
        ViewBag.TopProducts = await _db.Products.OrderByDescending(p => p.SalesCount).Take(5).ToListAsync();
        return View();
    }
}
