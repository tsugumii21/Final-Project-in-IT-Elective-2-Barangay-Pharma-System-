using BarangayPharmaSystem.Areas.Staff.Models;
using BarangayPharmaSystem.Data;
using BarangayPharmaSystem.Models.Entities;
using BarangayPharmaSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BarangayPharmaSystem.Areas.Staff.Controllers;

[Area("Staff")]
[Authorize(Roles = "Staff,Admin")]
public class DispensingController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAuditService _auditService;
    private readonly UserManager<ApplicationUser> _userManager;

    public DispensingController(AppDbContext db, IAuditService auditService, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _auditService = auditService;
        _userManager = userManager;
    }

    // ── Index ─────────────────────────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        var records = await _db.DispensingRecords
            .Include(d => d.Patient)
            .Include(d => d.Staff)
            .Include(d => d.Prescription)
            .ThenInclude(p => p.Medicine)
            .OrderByDescending(d => d.DateDispensed)
            .ToListAsync();

        var model = records.Select(d => new StaffDispensingListViewModel
        {
            Id = d.Id,
            DateDispensed = d.DateDispensed,
            PatientName = d.Patient.FullName,
            MedicineName = d.Prescription.Medicine.Name,
            QuantityDispensed = d.QuantityDispensed,
            StaffName = d.Staff.FullName
        }).ToList();

        return View(model);
    }

    // ── Create ────────────────────────────────────────────────────────────────
    public async Task<IActionResult> Create(int? prescriptionId)
    {
        await LoadPrescriptionsDropdownAsync(prescriptionId);
        return View(new StaffDispensingFormViewModel { PrescriptionId = prescriptionId ?? 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StaffDispensingFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadPrescriptionsDropdownAsync(model.PrescriptionId);
            return View(model);
        }

        var prescription = await _db.Prescriptions
            .Include(p => p.Medicine)
            .Include(p => p.Patient)
            .FirstOrDefaultAsync(p => p.Id == model.PrescriptionId);

        if (prescription == null || prescription.Status != PrescriptionStatus.Active && prescription.Status != PrescriptionStatus.Refilled)
        {
            ModelState.AddModelError("", "Selected prescription is invalid or not active.");
            await LoadPrescriptionsDropdownAsync(model.PrescriptionId);
            return View(model);
        }

        if (prescription.Medicine.IsExpired)
        {
            ModelState.AddModelError("", "Cannot dispense: the prescribed medicine has expired.");
            await LoadPrescriptionsDropdownAsync(model.PrescriptionId);
            return View(model);
        }

        if (model.Quantity > prescription.Medicine.Stock)
        {
            ModelState.AddModelError("Quantity", $"Insufficient stock! Only {prescription.Medicine.Stock} units available.");
            await LoadPrescriptionsDropdownAsync(model.PrescriptionId);
            return View(model);
        }

        var staffId = _userManager.GetUserId(User);

        // Deduct Stock
        prescription.Medicine.Stock -= model.Quantity;
        _db.Medicines.Update(prescription.Medicine);

        // Create Record
        var record = new DispensingRecord
        {
            PrescriptionId = prescription.Id,
            PatientId = prescription.PatientId,
            StaffId = staffId ?? "",
            QuantityDispensed = model.Quantity,
            DateDispensed = DateTime.UtcNow,
            Notes = model.Notes
        };

        _db.DispensingRecords.Add(record);

        // Stock Alert Logic
        await HandleStockAlertsAsync(prescription.Medicine);

        await _db.SaveChangesAsync();

        await _auditService.LogAsync("Created", "DispensingRecords", record.Id.ToString(), $"Staff dispensed {model.Quantity} of '{prescription.Medicine.Name}' to '{prescription.Patient.FullName}'.");

        TempData["SuccessMessage"] = $"Successfully dispensed {model.Quantity} units of {prescription.Medicine.Name}.";
        return RedirectToAction(nameof(Index));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private async Task LoadPrescriptionsDropdownAsync(int? selectedId)
    {
        var prescriptions = await _db.Prescriptions
            .Include(p => p.Patient)
            .Include(p => p.Medicine)
            .Where(p => (p.Status == PrescriptionStatus.Active || p.Status == PrescriptionStatus.Refilled) && p.Medicine.ExpiryDate >= DateTime.Today)
            .OrderBy(p => p.Patient.FullName)
            .Select(p => new { 
                p.Id, 
                DisplayText = $"{p.Patient.FullName} - {p.Medicine.Name} (Stock: {p.Medicine.Stock})" 
            })
            .ToListAsync();

        ViewBag.Prescriptions = new SelectList(prescriptions, "Id", "DisplayText", selectedId);
    }

    private async Task HandleStockAlertsAsync(Medicine medicine)
    {
        var existingAlert = await _db.StockAlerts
            .FirstOrDefaultAsync(s => s.MedicineId == medicine.Id && !s.IsResolved);

        bool createAlert = false;
        AlertType alertType = AlertType.LowStock;
        string message = "";

        if (medicine.Stock == 0)
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

                _db.StockAlerts.Add(new StockAlert
                {
                    MedicineId = medicine.Id,
                    AlertType = alertType,
                    Message = message,
                    IsResolved = false,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
    }
}
