using BarangayPharmaSystem.Areas.Patient.Models;
using BarangayPharmaSystem.Data;
using BarangayPharmaSystem.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using PatientEntity = BarangayPharmaSystem.Models.Entities.Patient;

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

        var activePrescriptions  = patient.Prescriptions.Count(p => p.Status == PrescriptionStatus.Active);
        var totalDispensing      = patient.DispensingRecords.Count;
        var pendingRefills       = patient.RefillRequests.Count(r => r.Status == RefillRequestStatus.Pending);
        var latestPrescription   = patient.Prescriptions.OrderByDescending(p => p.CreatedAt).FirstOrDefault();

        // ── Build notification list for the layout bell ────────────────────────
        var notifications = BuildNotifications(patient);
        ViewData["PatientNotifications"] = notifications;
        ViewData["PatientNotifCount"]    = notifications.Count;

        var model = new PatientDashboardViewModel
        {
            PatientName               = patient.FullName,
            ProfilePhotoPath          = patient.ProfilePhotoPath,
            ActivePrescriptions       = activePrescriptions,
            TotalDispensingRecords    = totalDispensing,
            PendingRefillRequests     = pendingRefills,
            LatestPrescription        = latestPrescription
        };

        return View(model);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static List<(string Icon, string Color, string Text, string Sub)> BuildNotifications(
        PatientEntity patient)

    {
        var items = new List<(string, string, string, string)>();

        // 1. Pending refill requests
        var pendingRefills = patient.RefillRequests
            .Where(r => r.Status == RefillRequestStatus.Pending)
            .ToList();
        foreach (var r in pendingRefills)
        {
            items.Add(("bi-arrow-repeat", "#D97706",
                "Refill request is pending review",
                $"Submitted {r.RequestDate:MMM dd, yyyy}"));
        }

        // 2. Approved or rejected refill requests in the last 7 days
        var recentDecided = patient.RefillRequests
            .Where(r => r.Status is RefillRequestStatus.Approved or RefillRequestStatus.Rejected
                        && r.RequestDate >= DateTime.UtcNow.AddDays(-7))
            .ToList();
        foreach (var r in recentDecided)
        {
            var isApproved = r.Status == RefillRequestStatus.Approved;
            items.Add((
                isApproved ? "bi-check-circle-fill" : "bi-x-circle-fill",
                isApproved ? "#16A34A" : "#DC2626",
                isApproved ? "Refill request approved" : "Refill request rejected",
                $"Updated {r.RequestDate:MMM dd, yyyy}"));
        }

        // 3. Active prescriptions expiring within 7 days (heuristic: created > 23 days ago and still Active)
        var expiringSoon = patient.Prescriptions
            .Where(p => p.Status == PrescriptionStatus.Active
                        && p.CreatedAt <= DateTime.UtcNow.AddDays(-23))
            .ToList();
        foreach (var p in expiringSoon)
        {
            items.Add(("bi-exclamation-triangle-fill", "#D97706",
                $"Prescription for {p.Medicine?.Name ?? "medicine"} may expire soon",
                "Contact staff to review"));
        }

        return items;
    }
}
