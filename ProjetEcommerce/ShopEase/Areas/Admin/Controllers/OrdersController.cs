using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopEase.Data;
using ShopEase.Models;

namespace ShopEase.Areas.Admin.Controllers;

/// <summary>Suivi des commandes et changement de statut.</summary>
[Area("Admin"), Authorize(Roles = DbSeeder.AdminRole)]
public class OrdersController : Controller
{
    private readonly ApplicationDbContext _db;
    public OrdersController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(OrderStatus? status)
    {
        var q = _db.Orders.Include(o => o.User).Include(o => o.Items).AsQueryable();
        if (status is not null) q = q.Where(o => o.Status == status);
        ViewBag.Status = status;
        return View(await q.OrderByDescending(o => o.OrderDate).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _db.Orders.Include(o => o.User).Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
        return order is null ? NotFound() : View(order);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
    {
        var order = await _db.Orders.Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (order is null) return NotFound();

        if (order.Status == OrderStatus.Cancelled)
        {
            TempData["Error"] = "Une commande annulée ne peut plus changer de statut.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Une annulation remet les articles en stock.
        if (status == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled)
            foreach (var item in order.Items)
            {
                item.Product!.Stock += item.Quantity;
                item.Product.SalesCount = Math.Max(0, item.Product.SalesCount - item.Quantity);
            }

        order.Status = status;
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Commande {order.OrderNumber} : statut mis à jour.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
