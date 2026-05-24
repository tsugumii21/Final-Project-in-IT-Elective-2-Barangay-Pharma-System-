using BarangayPharmaSystem.Areas.Patient.Models;
using BarangayPharmaSystem.Data;
using BarangayPharmaSystem.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BarangayPharmaSystem.Areas.Patient.Controllers;

[Area("Patient")]
[Authorize(Roles = "Patient")]
public class RefillsController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public RefillsController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // ── Index ─────────────────────────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.LinkedUserId == userId);

        if (patient == null) return RedirectToAction("Index", "Dashboard", new { area = "Patient" });

        var requests = await _db.RefillRequests
            .Include(r => r.Prescription)
                .ThenInclude(p => p.Medicine)
            .Where(r => r.PatientId == patient.Id)
            .OrderByDescending(r => r.RequestDate)
            .ToListAsync();

        var model = requests.Select(r => new PatientRefillRequestListViewModel
        {
            Id = r.Id,
            RequestDate = r.RequestDate,
            MedicineName = r.Prescription.Medicine.Name,
            Dosage = r.Prescription.Dosage,
            Notes = r.Notes,
            Status = r.Status
        }).ToList();

        return View(model);
    }

    // ── Create ────────────────────────────────────────────────────────────────
    public async Task<IActionResult> Create(int prescriptionId)
    {
        var userId = _userManager.GetUserId(User);
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.LinkedUserId == userId);

        if (patient == null) return RedirectToAction("Index", "Dashboard", new { area = "Patient" });

        var prescription = await _db.Prescriptions
            .Include(p => p.Medicine)
            .Include(p => p.DispensingRecords)
            .Include(p => p.RefillRequests)
            .FirstOrDefaultAsync(p => p.Id == prescriptionId && p.PatientId == patient.Id);

        if (prescription == null) return NotFound();

        // Server-Side Cooldown & Validation Logic
        if (prescription.Status != PrescriptionStatus.Active && prescription.Status != PrescriptionStatus.Refilled)
        {
            TempData["ErrorMessage"] = "This prescription is not active. Refills are not allowed.";
            return RedirectToAction("Index", "Prescriptions");
        }

        if (prescription.Medicine.IsExpired)
        {
            TempData["ErrorMessage"] = "Refill blocked: The prescribed medicine has expired.";
            return RedirectToAction("Index", "Prescriptions");
        }

        if (prescription.RefillRequests.Any(r => r.Status == RefillRequestStatus.Pending))
        {
            TempData["ErrorMessage"] = "You already have a Pending refill request for this prescription.";
            return RedirectToAction("Index", "Prescriptions");
        }

        var latestDispensing = prescription.DispensingRecords.OrderByDescending(d => d.DateDispensed).FirstOrDefault();
        if (latestDispensing != null)
        {
            var daysSinceDispensed = (DateTime.UtcNow - latestDispensing.DateDispensed).TotalDays;
            if (daysSinceDispensed < 20)
            {
                var availableDate = latestDispensing.DateDispensed.AddDays(20);
                TempData["ErrorMessage"] = $"Refill blocked: Cooldown period active. You can request a refill after {availableDate.ToLocalTime():MMM dd, yyyy}.";
                return RedirectToAction("Index", "Prescriptions");
            }
        }
        else
        {
            TempData["ErrorMessage"] = "Refill blocked: Medicine has not been dispensed yet. Please visit the pharmacy for your initial supply.";
            return RedirectToAction("Index", "Prescriptions");
        }

        var model = new PatientRefillRequestFormViewModel
        {
            PrescriptionId = prescription.Id,
            MedicineName = prescription.Medicine.Name,
            Dosage = prescription.Dosage
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PatientRefillRequestFormViewModel model)
    {
        var userId = _userManager.GetUserId(User);
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.LinkedUserId == userId);

        if (patient == null) return RedirectToAction("Index", "Dashboard", new { area = "Patient" });

        var prescription = await _db.Prescriptions
            .Include(p => p.Medicine)
            .Include(p => p.DispensingRecords)
            .Include(p => p.RefillRequests)
            .FirstOrDefaultAsync(p => p.Id == model.PrescriptionId && p.PatientId == patient.Id);

        if (prescription == null) return NotFound();

        // ── Double-check Server-Side Cooldown & Validation Logic on POST ──
        if (prescription.Status != PrescriptionStatus.Active && prescription.Status != PrescriptionStatus.Refilled)
        {
            TempData["ErrorMessage"] = "This prescription is not active.";
            return RedirectToAction("Index", "Prescriptions");
        }

        if (prescription.Medicine.IsExpired)
        {
            TempData["ErrorMessage"] = "The prescribed medicine has expired.";
            return RedirectToAction("Index", "Prescriptions");
        }

        if (prescription.RefillRequests.Any(r => r.Status == RefillRequestStatus.Pending))
        {
            TempData["ErrorMessage"] = "You already have a Pending refill request.";
            return RedirectToAction("Index", "Prescriptions");
        }

        var latestDispensing = prescription.DispensingRecords.OrderByDescending(d => d.DateDispensed).FirstOrDefault();
        if (latestDispensing != null)
        {
            var daysSinceDispensed = (DateTime.UtcNow - latestDispensing.DateDispensed).TotalDays;
            if (daysSinceDispensed < 20)
            {
                var availableDate = latestDispensing.DateDispensed.AddDays(20);
                TempData["ErrorMessage"] = $"Cooldown period active. You can request after {availableDate.ToLocalTime():MMM dd, yyyy}.";
                return RedirectToAction("Index", "Prescriptions");
            }
        }
        else
        {
            TempData["ErrorMessage"] = "Medicine has not been dispensed yet.";
            return RedirectToAction("Index", "Prescriptions");
        }
        // ─────────────────────────────────────────────────────────────────

        var request = new RefillRequest
        {
            PrescriptionId = prescription.Id,
            PatientId = patient.Id,
            RequestDate = DateTime.UtcNow,
            Notes = model.Notes,
            Status = RefillRequestStatus.Pending
        };

        _db.RefillRequests.Add(request);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Refill request submitted successfully! Staff will review it shortly.";
        return RedirectToAction(nameof(Index));
    }
}
