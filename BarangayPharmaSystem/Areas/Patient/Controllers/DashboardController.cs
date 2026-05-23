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
public class DashboardController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        
        // Find the patient profile linked to this user account
        var patient = await _db.Patients
            .Include(p => p.Prescriptions)
                .ThenInclude(pr => pr.Medicine)
            .Include(p => p.DispensingRecords)
            .Include(p => p.RefillRequests)
            .FirstOrDefaultAsync(p => p.LinkedUserId == userId);

        if (patient == null)
        {
            // Edge case: Patient role user but no linked profile
            return View("NoProfileFound");
        }

        var activePrescriptions = patient.Prescriptions.Count(p => p.Status == PrescriptionStatus.Active);
        var totalDispensing = patient.DispensingRecords.Count;
        var pendingRefills = patient.RefillRequests.Count(r => r.Status == RefillRequestStatus.Pending);
        
        var latestPrescription = patient.Prescriptions.OrderByDescending(p => p.CreatedAt).FirstOrDefault();

        var model = new PatientDashboardViewModel
        {
            PatientName = patient.FullName,
            ProfilePhotoPath = patient.ProfilePhotoPath,
            ActivePrescriptions = activePrescriptions,
            TotalDispensingRecords = totalDispensing,
            PendingRefillRequests = pendingRefills,
            LatestPrescription = latestPrescription
        };

        return View(model);
    }
}
