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
public class PrescriptionsController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAuditService _auditService;
    private readonly UserManager<ApplicationUser> _userManager;

    public PrescriptionsController(AppDbContext db, IAuditService auditService, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _auditService = auditService;
        _userManager = userManager;
    }

    // ── Index ─────────────────────────────────────────────────────────────────
    public async Task<IActionResult> Index(string? searchPatient, PrescriptionStatus? statusFilter)
    {
        var query = _db.Prescriptions
            .Include(p => p.Patient)
            .Include(p => p.Medicine)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchPatient))
        {
            searchPatient = searchPatient.Trim().ToLower();
            query = query.Where(p => p.Patient.FullName.ToLower().Contains(searchPatient) || p.Patient.PatientCode.ToLower().Contains(searchPatient));
        }

        if (statusFilter.HasValue)
        {
            query = query.Where(p => p.Status == statusFilter.Value);
        }

        var prescriptions = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();

        var model = prescriptions.Select(p => new StaffPrescriptionListViewModel
        {
            Id = p.Id,
            PatientName = p.Patient.FullName,
            PatientCode = p.Patient.PatientCode,
            MedicineName = p.Medicine.Name,
            DoctorName = p.DoctorName,
            Dosage = p.Dosage,
            Duration = p.Duration,
            Status = p.Status,
            CreatedAt = p.CreatedAt
        }).ToList();

        ViewData["SearchPatient"] = searchPatient;
        ViewData["StatusFilter"] = statusFilter;

        return View(model);
    }

    // ── Create ────────────────────────────────────────────────────────────────
    public async Task<IActionResult> Create(int? patientId)
    {
        await LoadDropdownsAsync(patientId);
        return View(new StaffPrescriptionFormViewModel { PatientId = patientId ?? 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StaffPrescriptionFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync(model.PatientId);
            return View(model);
        }

        var staffId = _userManager.GetUserId(User);

        var prescription = new Prescription
        {
            PatientId = model.PatientId,
            MedicineId = model.MedicineId,
            StaffId = staffId ?? "",
            DoctorName = model.DoctorName,
            Dosage = model.Dosage,
            Duration = model.Duration,
            Status = PrescriptionStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        _db.Prescriptions.Add(prescription);
        await _db.SaveChangesAsync();

        var patient = await _db.Patients.FindAsync(model.PatientId);
        var medicine = await _db.Medicines.FindAsync(model.MedicineId);

        await _auditService.LogAsync("Created", "Prescriptions", prescription.Id.ToString(), $"Staff created prescription for '{patient?.FullName}' - Medicine: {medicine?.Name}.");

        TempData["SuccessMessage"] = "Prescription created successfully!";
        return RedirectToAction(nameof(Index));
    }

    // ── Edit ──────────────────────────────────────────────────────────────────
    public async Task<IActionResult> Edit(int id)
    {
        var prescription = await _db.Prescriptions.FindAsync(id);
        if (prescription == null) return NotFound();

        // Only Active prescriptions can be edited to maintain data integrity
        if (prescription.Status != PrescriptionStatus.Active)
        {
            TempData["ErrorMessage"] = "Only 'Active' prescriptions can be edited.";
            return RedirectToAction(nameof(Index));
        }

        var model = new StaffPrescriptionFormViewModel
        {
            Id = prescription.Id,
            PatientId = prescription.PatientId,
            MedicineId = prescription.MedicineId,
            DoctorName = prescription.DoctorName,
            Dosage = prescription.Dosage,
            Duration = prescription.Duration,
            Status = prescription.Status
        };

        await LoadDropdownsAsync(prescription.PatientId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(StaffPrescriptionFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync(model.PatientId);
            return View(model);
        }

        var prescription = await _db.Prescriptions.FindAsync(model.Id);
        if (prescription == null) return NotFound();

        if (prescription.Status != PrescriptionStatus.Active)
        {
            TempData["ErrorMessage"] = "Only 'Active' prescriptions can be edited.";
            return RedirectToAction(nameof(Index));
        }

        prescription.PatientId = model.PatientId;
        prescription.MedicineId = model.MedicineId;
        prescription.DoctorName = model.DoctorName;
        prescription.Dosage = model.Dosage;
        prescription.Duration = model.Duration;
        prescription.Status = model.Status;

        _db.Prescriptions.Update(prescription);
        await _db.SaveChangesAsync();

        await _auditService.LogAsync("Updated", "Prescriptions", prescription.Id.ToString(), $"Staff updated prescription #{prescription.Id}.");

        TempData["SuccessMessage"] = "Prescription updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    // ── Delete ────────────────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var prescription = await _db.Prescriptions.Include(p => p.DispensingRecords).FirstOrDefaultAsync(p => p.Id == id);
        if (prescription == null) return NotFound();

        if (prescription.DispensingRecords.Any())
        {
            TempData["ErrorMessage"] = "Cannot delete a prescription that already has dispensing records.";
            return RedirectToAction(nameof(Index));
        }

        prescription.IsDeleted = true;
        _db.Prescriptions.Update(prescription);
        await _db.SaveChangesAsync();

        await _auditService.LogAsync("Deleted", "Prescriptions", prescription.Id.ToString(), $"Staff deleted prescription #{prescription.Id}.");

        TempData["SuccessMessage"] = "Prescription deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private async Task LoadDropdownsAsync(int? selectedPatientId = null)
    {
        var patients = await _db.Patients
            .OrderBy(p => p.FullName)
            .Select(p => new { p.Id, DisplayText = $"{p.PatientCode} - {p.FullName}" })
            .ToListAsync();
        ViewBag.Patients = new SelectList(patients, "Id", "DisplayText", selectedPatientId);

        var medicines = await _db.Medicines
            .OrderBy(m => m.Name)
            .Select(m => new { m.Id, DisplayText = $"{m.Name} (Stock: {m.Stock})" })
            .ToListAsync();
        ViewBag.Medicines = new SelectList(medicines, "Id", "DisplayText");
    }
}
