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
public class PrescriptionsController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public PrescriptionsController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(PrescriptionStatus? statusFilter)
    {
        var userId = _userManager.GetUserId(User);
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.LinkedUserId == userId);

        if (patient == null) return RedirectToAction("Index", "Dashboard", new { area = "Patient" });

        var query = _db.Prescriptions
            .Include(p => p.Medicine)
            .Include(p => p.DispensingRecords)
            .Include(p => p.RefillRequests)
            .Where(p => p.PatientId == patient.Id);

        if (statusFilter.HasValue)
        {
            query = query.Where(p => p.Status == statusFilter.Value);
        }

        var prescriptions = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();

        var model = new List<PatientPrescriptionListViewModel>();
        var now = DateTime.UtcNow;

        foreach (var p in prescriptions)
        {
            var vm = new PatientPrescriptionListViewModel
            {
                Id = p.Id,
                MedicineName = p.Medicine.Name,
                DoctorName = p.DoctorName,
                Dosage = p.Dosage,
                Duration = p.Duration,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                CanRequestRefill = false,
                RefillBlockReason = null
            };

            // Cooldown Logic
            if (p.Status != PrescriptionStatus.Active && p.Status != PrescriptionStatus.Refilled)
            {
                vm.RefillBlockReason = "Prescription is not Active.";
            }
            else if (p.Medicine.IsExpired)
            {
                vm.RefillBlockReason = "The prescribed medicine has expired.";
            }
            else if (p.RefillRequests.Any(r => r.Status == RefillRequestStatus.Pending))
            {
                vm.RefillBlockReason = "You already have a Pending refill request for this prescription.";
            }
            else
            {
                var latestDispensing = p.DispensingRecords.OrderByDescending(d => d.DateDispensed).FirstOrDefault();
                if (latestDispensing != null)
                {
                    var daysSinceDispensed = (now - latestDispensing.DateDispensed).TotalDays;
                    if (daysSinceDispensed < 20)
                    {
                        var availableDate = latestDispensing.DateDispensed.AddDays(20);
                        vm.RefillBlockReason = $"You can request a refill after {availableDate.ToLocalTime():MMM dd, yyyy}.";
                    }
                    else
                    {
                        vm.CanRequestRefill = true;
                    }
                }
                else
                {
                    // Has never been dispensed. Usually shouldn't request a refill if not dispensed, 
                    // but we allow it if there's no dispensing record yet (or we could block it).
                    // Let's block it since it's a "Refill"
                    vm.RefillBlockReason = "Medicine has not been dispensed yet. Please visit the pharmacy for your initial supply.";
                }
            }

            model.Add(vm);
        }

        ViewData["StatusFilter"] = statusFilter;
        return View(model);
    }
}
