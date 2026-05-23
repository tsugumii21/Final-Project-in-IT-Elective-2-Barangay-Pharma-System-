using BarangayPharmaSystem.Areas.Admin.Models;
using BarangayPharmaSystem.Data;
using BarangayPharmaSystem.Models.Entities;
using BarangayPharmaSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BarangayPharmaSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class SuppliersController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAuditService _auditService;
    private readonly UserManager<ApplicationUser> _userManager;

    public SuppliersController(AppDbContext db, IAuditService auditService, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _auditService = auditService;
        _userManager = userManager;
    }

    // ── Index ─────────────────────────────────────────────────────────────────
    public async Task<IActionResult> Index(string? search)
    {
        var query = _db.Suppliers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim().ToLower();
            query = query.Where(s => s.Name.ToLower().Contains(search) || 
                                     (s.ContactPerson != null && s.ContactPerson.ToLower().Contains(search)));
        }

        var suppliers = await query.OrderBy(s => s.Name)
            .Select(s => new SupplierListViewModel
            {
                Id = s.Id,
                Name = s.Name,
                ContactPerson = s.ContactPerson,
                ContactNumber = s.ContactNumber
            })
            .ToListAsync();

        ViewData["SearchQuery"] = search;
        return View(suppliers);
    }

    // ── Create ────────────────────────────────────────────────────────────────
    public IActionResult Create()
    {
        return View(new SupplierFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SupplierFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var supplier = new Supplier
        {
            Name = model.Name,
            ContactPerson = model.ContactPerson,
            ContactNumber = model.ContactNumber,
            Address = model.Address,
            CreatedAt = DateTime.UtcNow
        };

        _db.Suppliers.Add(supplier);
        await _db.SaveChangesAsync();

        await _auditService.LogAsync("Created", "Suppliers", supplier.Id.ToString(), $"Admin created supplier '{supplier.Name}'.");

        TempData["SuccessMessage"] = $"Supplier '{supplier.Name}' added successfully!";
        return RedirectToAction(nameof(Index));
    }

    // ── Edit ──────────────────────────────────────────────────────────────────
    public async Task<IActionResult> Edit(int id)
    {
        var supplier = await _db.Suppliers.FindAsync(id);
        if (supplier == null) return NotFound();

        var model = new SupplierFormViewModel
        {
            Id = supplier.Id,
            Name = supplier.Name,
            ContactPerson = supplier.ContactPerson,
            ContactNumber = supplier.ContactNumber,
            Address = supplier.Address
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SupplierFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var supplier = await _db.Suppliers.FindAsync(model.Id);
        if (supplier == null) return NotFound();

        supplier.Name = model.Name;
        supplier.ContactPerson = model.ContactPerson;
        supplier.ContactNumber = model.ContactNumber;
        supplier.Address = model.Address;

        _db.Suppliers.Update(supplier);
        await _db.SaveChangesAsync();

        await _auditService.LogAsync("Updated", "Suppliers", supplier.Id.ToString(), $"Admin updated supplier '{supplier.Name}'.");

        TempData["SuccessMessage"] = $"Supplier '{supplier.Name}' updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    // ── Delete ────────────────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var supplier = await _db.Suppliers.FindAsync(id);
        if (supplier == null) return NotFound();

        supplier.IsDeleted = true;
        _db.Suppliers.Update(supplier);
        await _db.SaveChangesAsync();

        await _auditService.LogAsync("Deleted", "Suppliers", supplier.Id.ToString(), $"Admin deleted supplier '{supplier.Name}'.");

        TempData["SuccessMessage"] = "Supplier deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
