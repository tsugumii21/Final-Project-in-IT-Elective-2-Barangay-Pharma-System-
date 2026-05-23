using BarangayPharmaSystem.Areas.Staff.Models;
using BarangayPharmaSystem.Data;
using BarangayPharmaSystem.Models.Entities;
using BarangayPharmaSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BarangayPharmaSystem.Areas.Staff.Controllers;

[Area("Staff")]
[Authorize(Roles = "Staff,Admin")]
public class RefillRequestsController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAuditService _auditService;
    private readonly UserManager<ApplicationUser> _userManager;

    public RefillRequestsController(AppDbContext db, IAuditService auditService, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _auditService = auditService;
        _userManager = userManager;
    }

    // ── Index ─────────────────────────────────────────────────────────────────
    public async Task<IActionResult> Index(RefillRequestStatus? statusFilter = RefillRequestStatus.Pending)
    {
        var query = _db.RefillRequests
            .Include(r => r.Patient)
            .Include(r => r.Prescription)
            .ThenInclude(p => p.Medicine)
            .AsQueryable();

        if (statusFilter.HasValue)
        {
            query = query.Where(r => r.Status == statusFilter.Value);
        }

        var requests = await query.OrderByDescending(r => r.RequestDate).ToListAsync();

        var model = requests.Select(r => new StaffRefillRequestListViewModel
        {
            Id = r.Id,
            RequestDate = r.RequestDate,
            PatientName = r.Patient.FullName,
            MedicineName = r.Prescription.Medicine.Name,
            PrescriptionDosage = r.Prescription.Dosage,
            PatientNotes = r.Notes,
            Status = r.Status,
            CurrentStock = r.Prescription.Medicine.Stock,
            PrescriptionId = r.PrescriptionId
        }).ToList();

        ViewData["StatusFilter"] = statusFilter;

        return View(model);
    }

    // ── Approve ───────────────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id, int quantityToDispense)
    {
        var request = await _db.RefillRequests
            .Include(r => r.Prescription)
            .ThenInclude(p => p.Medicine)
            .Include(r => r.Patient)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null || request.Status != RefillRequestStatus.Pending)
        {
            TempData["ErrorMessage"] = "Request not found or is no longer pending.";
            return RedirectToAction(nameof(Index));
        }

        if (quantityToDispense <= 0)
        {
            TempData["ErrorMessage"] = "Invalid dispensing quantity.";
            return RedirectToAction(nameof(Index));
        }

        if (quantityToDispense > request.Prescription.Medicine.Stock)
        {
            TempData["ErrorMessage"] = $"Insufficient stock. Only {request.Prescription.Medicine.Stock} left.";
            return RedirectToAction(nameof(Index));
        }

        var staffId = _userManager.GetUserId(User);

        // Deduct Stock
        request.Prescription.Medicine.Stock -= quantityToDispense;

        // Create Dispensing Record
        var record = new DispensingRecord
        {
            PrescriptionId = request.PrescriptionId,
            PatientId = request.PatientId,
            StaffId = staffId ?? "",
            QuantityDispensed = quantityToDispense,
            DateDispensed = DateTime.UtcNow,
            Notes = "Dispensed via Refill Request Approval."
        };
        _db.DispensingRecords.Add(record);

        // Update Request
        request.Status = RefillRequestStatus.Approved;
        _db.RefillRequests.Update(request);

        // Update Prescription Status
        request.Prescription.Status = PrescriptionStatus.Refilled;
        _db.Prescriptions.Update(request.Prescription);

        // Handle Alerts
        await HandleStockAlertsAsync(request.Prescription.Medicine);

        await _db.SaveChangesAsync();

        await _auditService.LogAsync("Updated", "RefillRequests", request.Id.ToString(), $"Staff approved refill #{request.Id} and dispensed {quantityToDispense} units.");

        TempData["SuccessMessage"] = $"Refill approved and {quantityToDispense} units dispensed.";
        return RedirectToAction(nameof(Index));
    }

    // ── Reject ────────────────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string rejectionNotes)
    {
        var request = await _db.RefillRequests.FindAsync(id);
        if (request == null || request.Status != RefillRequestStatus.Pending)
        {
            TempData["ErrorMessage"] = "Request not found or is no longer pending.";
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(rejectionNotes))
        {
            TempData["ErrorMessage"] = "Rejection notes are required.";
            return RedirectToAction(nameof(Index));
        }

        request.Status = RefillRequestStatus.Rejected;
        request.Notes = string.IsNullOrWhiteSpace(request.Notes) 
            ? $"Rejected: {rejectionNotes}" 
            : $"{request.Notes}\n\n[Staff Rejection]: {rejectionNotes}";

        _db.RefillRequests.Update(request);
        await _db.SaveChangesAsync();

        await _auditService.LogAsync("Updated", "RefillRequests", request.Id.ToString(), $"Staff rejected refill #{request.Id}. Reason: {rejectionNotes}");

        TempData["SuccessMessage"] = "Refill request rejected.";
        return RedirectToAction(nameof(Index));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
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
