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
public class PatientsController : Controller
{
    private readonly AppDbContext _db;
    private readonly IFileUploadService _fileUploadService;
    private readonly IAuditService _auditService;
    private readonly UserManager<ApplicationUser> _userManager;

    public PatientsController(
        AppDbContext db,
        IFileUploadService fileUploadService,
        IAuditService auditService,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _fileUploadService = fileUploadService;
        _auditService = auditService;
        _userManager = userManager;
    }

    // ── Index ─────────────────────────────────────────────────────────────────
    public async Task<IActionResult> Index(string? search)
    {
        var query = _db.Patients.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim().ToLower();
            query = query.Where(p => 
                p.FullName.ToLower().Contains(search) || 
                p.PatientCode.ToLower().Contains(search) || 
                (p.ContactNumber != null && p.ContactNumber.Contains(search)));
        }

        var patients = await query.OrderBy(p => p.FullName).ToListAsync();

        var model = patients.Select(p => new StaffPatientListViewModel
        {
            Id = p.Id,
            PatientCode = p.PatientCode,
            FullName = p.FullName,
            ContactNumber = p.ContactNumber,
            Age = p.Age,
            ProfilePhotoPath = p.ProfilePhotoPath,
            HasLinkedAccount = !string.IsNullOrEmpty(p.LinkedUserId)
        }).ToList();

        ViewData["SearchQuery"] = search;
        return View(model);
    }

    // ── Details ───────────────────────────────────────────────────────────────
    public async Task<IActionResult> Details(int id)
    {
        var patient = await _db.Patients
            .Include(p => p.Prescriptions.Where(x => !x.IsDeleted))
                .ThenInclude(pr => pr.Medicine)
            .Include(p => p.DispensingRecords)
                .ThenInclude(d => d.Prescription)
                .ThenInclude(pr => pr.Medicine)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (patient == null) return NotFound();

        var model = new StaffPatientDetailsViewModel
        {
            Patient = patient,
            Prescriptions = patient.Prescriptions.OrderByDescending(p => p.CreatedAt).ToList(),
            DispensingHistory = patient.DispensingRecords.OrderByDescending(d => d.DateDispensed).ToList()
        };

        return View(model);
    }

    // ── Create ────────────────────────────────────────────────────────────────
    public IActionResult Create()
    {
        return View(new StaffPatientFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StaffPatientFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        // Generate PatientCode PAT-YYYY-NNNNN
        int currentYear = DateTime.UtcNow.Year;
        var lastPatient = await _db.Patients
            .IgnoreQueryFilters()
            .Where(p => p.PatientCode.StartsWith($"PAT-{currentYear}-"))
            .OrderByDescending(p => p.Id)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastPatient != null)
        {
            var parts = lastPatient.PatientCode.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out int lastNum))
            {
                nextNumber = lastNum + 1;
            }
        }

        string patientCode = $"PAT-{currentYear}-{nextNumber:D5}";

        var patient = new Patient
        {
            PatientCode = patientCode,
            FullName = model.FullName,
            Birthdate = model.Birthdate,
            ContactNumber = model.ContactNumber ?? "",
            Address = model.Address,
            CreatedAt = DateTime.UtcNow
        };

        _db.Patients.Add(patient);
        await _db.SaveChangesAsync(); // save to get ID for photo

        if (model.PhotoFile != null)
        {
            var uploadResult = await _fileUploadService.UploadPatientPhotoAsync(model.PhotoFile, patient.Id);
            if (uploadResult.Success)
            {
                patient.ProfilePhotoPath = uploadResult.RelativePath;
                _db.Patients.Update(patient);
                await _db.SaveChangesAsync();
            }
        }

        await _auditService.LogAsync("Created", "Patients", patient.Id.ToString(), $"Staff created patient profile '{patient.FullName}' ({patientCode}).");

        TempData["SuccessMessage"] = $"Patient {patient.FullName} created! ID: {patientCode}";
        TempData["PatientCodeAlert"] = patientCode; // Useful for UI popup
        return RedirectToAction(nameof(Index));
    }

    // ── Edit ──────────────────────────────────────────────────────────────────
    public async Task<IActionResult> Edit(int id)
    {
        var patient = await _db.Patients.FindAsync(id);
        if (patient == null) return NotFound();

        var model = new StaffPatientFormViewModel
        {
            Id = patient.Id,
            PatientCode = patient.PatientCode,
            FullName = patient.FullName,
            Birthdate = patient.Birthdate,
            ContactNumber = patient.ContactNumber,
            Address = patient.Address,
            CurrentPhotoPath = patient.ProfilePhotoPath
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(StaffPatientFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var patient = await _db.Patients.FindAsync(model.Id);
        if (patient == null) return NotFound();

        patient.FullName = model.FullName;
        patient.Birthdate = model.Birthdate;
        patient.ContactNumber = model.ContactNumber ?? "";
        patient.Address = model.Address;

        if (model.PhotoFile != null)
        {
            var uploadResult = await _fileUploadService.UploadPatientPhotoAsync(model.PhotoFile, patient.Id);
            if (uploadResult.Success)
            {
                patient.ProfilePhotoPath = uploadResult.RelativePath;
            }
        }

        _db.Patients.Update(patient);
        await _db.SaveChangesAsync();

        // Also update linked user's name if they have an account
        if (!string.IsNullOrEmpty(patient.LinkedUserId))
        {
            var user = await _userManager.FindByIdAsync(patient.LinkedUserId);
            if (user != null)
            {
                user.FullName = patient.FullName;
                user.PhoneNumber = patient.ContactNumber;
                if (model.PhotoFile != null)
                {
                    user.ProfilePhotoPath = patient.ProfilePhotoPath;
                }
                await _userManager.UpdateAsync(user);
            }
        }

        await _auditService.LogAsync("Updated", "Patients", patient.Id.ToString(), $"Staff updated patient profile '{patient.FullName}'.");

        TempData["SuccessMessage"] = "Patient profile updated successfully!";
        return RedirectToAction(nameof(Details), new { id = patient.Id });
    }

    // ── Delete (Soft) ─────────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var patient = await _db.Patients.FindAsync(id);
        if (patient == null) return NotFound();

        patient.IsDeleted = true;
        _db.Patients.Update(patient);
        await _db.SaveChangesAsync();

        // Soft delete linked user if exists
        if (!string.IsNullOrEmpty(patient.LinkedUserId))
        {
            var user = await _userManager.FindByIdAsync(patient.LinkedUserId);
            if (user != null)
            {
                user.IsDeleted = true;
                await _userManager.UpdateAsync(user);
            }
        }

        await _auditService.LogAsync("Deleted", "Patients", patient.Id.ToString(), $"Staff deleted patient profile '{patient.FullName}'.");

        TempData["SuccessMessage"] = "Patient profile deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
