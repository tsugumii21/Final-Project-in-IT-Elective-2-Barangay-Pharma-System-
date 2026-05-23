using BarangayPharmaSystem.Areas.Admin.Models;
using BarangayPharmaSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BarangayPharmaSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var model = new AdminDashboardViewModel
        {
            TotalPatients = await _db.Patients.CountAsync(p => !p.IsDeleted),
            TotalMedicines = await _db.Medicines.CountAsync(m => !m.IsDeleted),
            TotalPrescriptions = await _db.Prescriptions.CountAsync(),
            TotalDispensingRecords = await _db.DispensingRecords.CountAsync(),

            ActiveAlerts = await _db.StockAlerts
                .Include(a => a.Medicine)
                .Where(a => !a.IsResolved)
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .ToListAsync(),

            RecentActivity = await _db.AuditLogs
                .Include(a => a.User)
                .OrderByDescending(a => a.Timestamp)
                .Take(10)
                .ToListAsync()
        };

        return View(model);
    }
}
