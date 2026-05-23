using BarangayPharmaSystem.Areas.Staff.Models;
using BarangayPharmaSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BarangayPharmaSystem.Areas.Staff.Controllers;

[Area("Staff")]
[Authorize(Roles = "Staff,Admin")]
public class MedicinesController : Controller
{
    private readonly AppDbContext _db;

    public MedicinesController(AppDbContext db)
    {
        _db = db;
    }

    // ── Index ─────────────────────────────────────────────────────────────────
    public async Task<IActionResult> Index(string? search, string? category)
    {
        var query = _db.Medicines.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim().ToLower();
            query = query.Where(m => m.Name.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(m => m.Category == category);
        }

        var medicines = await query.OrderBy(m => m.Name).ToListAsync();

        var model = medicines.Select(m => new StaffInventoryViewModel
        {
            Id = m.Id,
            Name = m.Name,
            Category = m.Category,
            Stock = m.Stock,
            MinStockLevel = m.MinStockLevel,
            ExpiryDate = m.ExpiryDate,
            PhotoPath = m.PhotoPath
        }).ToList();

        ViewData["SearchQuery"] = search;
        ViewData["CategoryFilter"] = category;

        var categories = await _db.Medicines.Select(m => m.Category).Distinct().ToListAsync();
        ViewBag.Categories = categories;

        return View(model);
    }
}
