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
public class HistoryController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public HistoryController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
    {
        var userId = _userManager.GetUserId(User);
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.LinkedUserId == userId);

        if (patient == null) return RedirectToAction("Index", "Dashboard", new { area = "Patient" });

        var query = _db.DispensingRecords
            .Include(d => d.Prescription)
                .ThenInclude(p => p.Medicine)
            .Include(d => d.Staff)
            .Where(d => d.PatientId == patient.Id);

        if (startDate.HasValue)
        {
            query = query.Where(d => d.DateDispensed.Date >= startDate.Value.Date);
        }
        if (endDate.HasValue)
        {
            query = query.Where(d => d.DateDispensed.Date <= endDate.Value.Date);
        }

        var records = await query.OrderByDescending(d => d.DateDispensed).ToListAsync();

        var model = records.Select(d => new PatientDispensingListViewModel
        {
            Id = d.Id,
            DateDispensed = d.DateDispensed,
            MedicineName = d.Prescription.Medicine.Name,
            QuantityDispensed = d.QuantityDispensed,
            StaffName = d.Staff.FullName
        }).ToList();

        ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
        ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");

        return View(model);
    }
}
