using BarangayPharmaSystem.Areas.Admin.Models;
using BarangayPharmaSystem.Data;
using BarangayPharmaSystem.Models.Entities;
using BarangayPharmaSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BarangayPharmaSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class MedicinesController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAuditService _auditService;
    private readonly IFileUploadService _fileUploadService;
    private readonly UserManager<ApplicationUser> _userManager;

    public MedicinesController(
        AppDbContext db,
        IAuditService auditService,
        IFileUploadService fileUploadService,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _auditService = auditService;
        _fileUploadService = fileUploadService;
        _userManager = userManager;
    }

    // ── Index ─────────────────────────────────────────────────────────────────
    public async Task<IActionResult> Index(string? search, string? filter)
    {
        var query = _db.Medicines.Include(m => m.Supplier).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim().ToLower();
            query = query.Where(m => m.Name.ToLower().Contains(search) || m.Category.ToLower().Contains(search));
        }

        var medicines = await query.OrderBy(m => m.Name).ToListAsync();
        
        var model = medicines.Select(m => new MedicineListViewModel
        {
            Id = m.Id,
            Name = m.Name,
            Category = m.Category,
            Stock = m.Stock,
            MinStockLevel = m.MinStockLevel,
            ExpiryDate = m.ExpiryDate,
            SupplierName = m.Supplier?.Name,
            PhotoPath = m.PhotoPath
        }).ToList();

        if (!string.IsNullOrEmpty(filter))
        {
            if (filter == "lowstock") model = model.Where(m => m.IsLowStock).ToList();
            if (filter == "expired") model = model.Where(m => m.IsExpired).ToList();
        }

        ViewData["SearchQuery"] = search;
        ViewData["FilterQuery"] = filter;
        return View(model);
    }

    // ── Create ────────────────────────────────────────────────────────────────
    public async Task<IActionResult> Create()
    {
        var model = new MedicineFormViewModel
        {
            ExpiryDate = DateTime.Today.AddYears(1),
            SuppliersList = await GetSuppliersSelectList()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MedicineFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.SuppliersList = await GetSuppliersSelectList();
            return View(model);
        }

        var medicine = new Medicine
        {
            Name = model.Name,
            Category = model.Category,
            Stock = model.Stock,
            MinStockLevel = model.MinStockLevel,
            ExpiryDate = model.ExpiryDate,
            DosageInfo = model.DosageInfo,
            SupplierId = model.SupplierId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Medicines.Add(medicine);
        await _db.SaveChangesAsync();

        // Handle Photo Upload after ID is generated
        if (model.PhotoFile != null)
        {
            var uploadResult = await _fileUploadService.UploadMedicinePhotoAsync(model.PhotoFile, medicine.Id);
            if (uploadResult.Success)
            {
                medicine.PhotoPath = uploadResult.RelativePath;
                await _db.SaveChangesAsync();
            }
        }

        // Check for immediate Stock Alert
        await CheckAndCreateStockAlert(medicine);

        await _auditService.LogAsync("Created", "Medicines", medicine.Id.ToString(), $"Admin added medicine '{medicine.Name}'.");

        TempData["SuccessMessage"] = $"Medicine '{medicine.Name}' added successfully!";
        return RedirectToAction(nameof(Index));
    }

    // ── Edit ──────────────────────────────────────────────────────────────────
    public async Task<IActionResult> Edit(int id)
    {
        var medicine = await _db.Medicines.FindAsync(id);
        if (medicine == null) return NotFound();

        var model = new MedicineFormViewModel
        {
            Id = medicine.Id,
            Name = medicine.Name,
            Category = medicine.Category,
            Stock = medicine.Stock,
            MinStockLevel = medicine.MinStockLevel,
            ExpiryDate = medicine.ExpiryDate,
            DosageInfo = medicine.DosageInfo,
            SupplierId = medicine.SupplierId,
            CurrentPhotoPath = medicine.PhotoPath,
            SuppliersList = await GetSuppliersSelectList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(MedicineFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.SuppliersList = await GetSuppliersSelectList();
            return View(model);
        }

        var medicine = await _db.Medicines.FindAsync(model.Id);
        if (medicine == null) return NotFound();

        medicine.Name = model.Name;
        medicine.Category = model.Category;
        medicine.Stock = model.Stock;
        medicine.MinStockLevel = model.MinStockLevel;
        medicine.ExpiryDate = model.ExpiryDate;
        medicine.DosageInfo = model.DosageInfo;
        medicine.SupplierId = model.SupplierId;

        if (model.PhotoFile != null)
        {
            var uploadResult = await _fileUploadService.UploadMedicinePhotoAsync(model.PhotoFile, medicine.Id);
            if (uploadResult.Success)
            {
                if (!string.IsNullOrEmpty(medicine.PhotoPath))
                {
                    _fileUploadService.DeleteFile(medicine.PhotoPath);
                }
                medicine.PhotoPath = uploadResult.RelativePath;
            }
        }

        _db.Medicines.Update(medicine);
        await _db.SaveChangesAsync();

        await CheckAndCreateStockAlert(medicine);

        await _auditService.LogAsync("Updated", "Medicines", medicine.Id.ToString(), $"Admin updated medicine '{medicine.Name}'.");

        TempData["SuccessMessage"] = $"Medicine '{medicine.Name}' updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    // ── Delete ────────────────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var medicine = await _db.Medicines.FindAsync(id);
        if (medicine == null) return NotFound();

        medicine.IsDeleted = true;
        _db.Medicines.Update(medicine);

        // Resolve active alerts for deleted medicine
        var activeAlerts = await _db.StockAlerts.Where(a => a.MedicineId == id && !a.IsResolved).ToListAsync();
        foreach (var alert in activeAlerts)
        {
            alert.IsResolved = true;
            alert.ResolvedAt = DateTime.UtcNow;
            _db.StockAlerts.Update(alert);
        }

        await _db.SaveChangesAsync();

        await _auditService.LogAsync("Deleted", "Medicines", medicine.Id.ToString(), $"Admin deleted medicine '{medicine.Name}'.");

        TempData["SuccessMessage"] = "Medicine deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<IEnumerable<SelectListItem>> GetSuppliersSelectList()
    {
        var suppliers = await _db.Suppliers.OrderBy(s => s.Name).ToListAsync();
        return suppliers.Select(s => new SelectListItem
        {
            Value = s.Id.ToString(),
            Text = s.Name
        });
    }

    private async Task CheckAndCreateStockAlert(Medicine medicine)
    {
        bool createAlert = false;
        AlertType alertType = AlertType.LowStock;
        string message = "";

        if (medicine.IsExpired)
        {
            createAlert = true;
            alertType = AlertType.Expired;
            message = "Medicine has expired.";
        }
        else if (medicine.IsNearExpiry)
        {
            createAlert = true;
            alertType = AlertType.NearExpiry;
            message = $"Medicine will expire on {medicine.ExpiryDate:MMM dd, yyyy}.";
        }
        else if (medicine.Stock == 0)
        {
            createAlert = true;
            alertType = AlertType.OutOfStock;
            message = "Medicine is out of stock.";
        }
        else if (medicine.Stock <= medicine.MinStockLevel)
        {
            createAlert = true;
            alertType = AlertType.LowStock;
            message = $"Stock is low ({medicine.Stock} left).";
        }

        // If there's an existing UNRESOLVED alert of a DIFFERENT type, resolve it first
        // If there's an existing UNRESOLVED alert of the SAME type, do nothing (keep it)
        var existingAlert = await _db.StockAlerts
            .Where(a => a.MedicineId == medicine.Id && !a.IsResolved)
            .FirstOrDefaultAsync();

        if (createAlert)
        {
            if (existingAlert == null || existingAlert.AlertType != alertType)
            {
                if (existingAlert != null)
                {
                    existingAlert.IsResolved = true;
                    existingAlert.ResolvedAt = DateTime.UtcNow;
                    _db.StockAlerts.Update(existingAlert);
                }

                var newAlert = new StockAlert
                {
                    MedicineId = medicine.Id,
                    AlertType = alertType,
                    Message = message,
                    IsResolved = false,
                    CreatedAt = DateTime.UtcNow
                };
                _db.StockAlerts.Add(newAlert);
            }
        }
        else if (existingAlert != null)
        {
            // Medicine is fine now (restocked, expiry updated), resolve existing alert
            existingAlert.IsResolved = true;
            existingAlert.ResolvedAt = DateTime.UtcNow;
            _db.StockAlerts.Update(existingAlert);
        }

        await _db.SaveChangesAsync();
    }
}
