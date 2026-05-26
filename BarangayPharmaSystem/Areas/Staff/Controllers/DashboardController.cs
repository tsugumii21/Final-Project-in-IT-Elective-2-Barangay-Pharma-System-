using BarangayPharmaSystem.Areas.Staff.Models;
using BarangayPharmaSystem.Data;
using BarangayPharmaSystem.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BarangayPharmaSystem.Areas.Staff.Controllers;

[Area("Staff")]
[Authorize(Roles = "Staff,Admin")]
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
        var staffId = _userManager.GetUserId(User);

        // My Patients Today (count distinct patients dispensed to by this staff today)
        var today = DateTime.UtcNow.Date;
        var myPatientsToday = await _db.DispensingRecords
            .Where(d => d.StaffId == staffId && d.DateDispensed.Date == today)
            .Select(d => d.PatientId)
            .Distinct()
            .CountAsync();

        // Pending Refill Requests
        var pendingRefills = await _db.RefillRequests
            .Where(r => r.Status == RefillRequestStatus.Pending)
            .CountAsync();

        // Active Stock Alerts (Stock <= MinStockLevel OR ExpiryDate <= 30 days from today)
        var todayDate = DateTime.Today;
        var lowStockAlerts = await _db.Medicines
            .Where(m => m.Stock <= m.MinStockLevel || m.ExpiryDate <= todayDate.AddDays(30))
            .CountAsync();

        // Today's dispensing activity for this staff
        var todaysActivity = await _db.DispensingRecords
            .Include(d => d.Patient)
            .Include(d => d.Prescription)
            .ThenInclude(p => p.Medicine)
            .Where(d => d.StaffId == staffId && d.DateDispensed.Date == today)
            .OrderByDescending(d => d.DateDispensed)
            .ToListAsync();

        var model = new StaffDashboardViewModel
        {
            MyPatientsToday = myPatientsToday,
            PendingRefillRequests = pendingRefills,
            LowStockAlerts = lowStockAlerts,
            TodaysDispensing = todaysActivity
        };

        return View(model);
    }
}
