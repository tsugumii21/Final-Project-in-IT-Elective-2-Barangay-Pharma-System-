using BarangayPharmaSystem.Areas.Admin.Models;
using BarangayPharmaSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BarangayPharmaSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ReportsController : Controller
{
    private readonly AppDbContext _db;

    public ReportsController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
    {
        // Default to last 30 days if not provided
        var start = startDate ?? DateTime.Today.AddDays(-30);
        var end = endDate ?? DateTime.Today;
        
        // Ensure end date covers the entire day
        var endOfDay = end.Date.AddDays(1).AddTicks(-1);

        var model = new ReportsViewModel
        {
            StartDate = start,
            EndDate = end
        };

        // 1. Dispensing Records Summary
        var dispensingQuery = _db.DispensingRecords
            .Include(d => d.Prescription)
            .ThenInclude(p => p.Medicine)
            .Where(d => d.DateDispensed >= start && d.DateDispensed <= endOfDay);

        model.TotalDispensedRecords = await dispensingQuery.CountAsync();
        model.TotalMedicinesDispensed = await dispensingQuery.SumAsync(d => (int?)d.QuantityDispensed) ?? 0;

        // 2. Medicine Usage Summary (Top 10 most dispensed medicines in date range)
        var usage = await dispensingQuery
            .GroupBy(d => new { d.Prescription.Medicine.Id, d.Prescription.Medicine.Name, d.Prescription.Medicine.Category })
            .Select(g => new MedicineUsageReport
            {
                MedicineName = g.Key.Name,
                Category = g.Key.Category,
                TotalDispensed = g.Sum(d => d.QuantityDispensed),
                TimesDispensed = g.Count()
            })
            .OrderByDescending(x => x.TotalDispensed)
            .Take(10)
            .ToListAsync();
            
        model.TopMedicines = usage;

        // 3. Low Stock Report (Current status, ignores date range)
        var lowStock = await _db.Medicines
            .Where(m => !m.IsDeleted && m.Stock <= m.MinStockLevel)
            .OrderBy(m => m.Stock)
            .Select(m => new LowStockReport
            {
                MedicineName = m.Name,
                CurrentStock = m.Stock,
                MinStockLevel = m.MinStockLevel,
                Status = m.Stock <= 0 ? "Out of Stock" : "Low Stock"
            })
            .ToListAsync();

        model.LowStockMedicines = lowStock;

        // 4. Patient Activity Report (Top 10 active patients)
        var patientActivity = await _db.Patients
            .Where(p => !p.IsDeleted)
            .Select(p => new PatientActivityReport
            {
                PatientName = p.FullName,
                PatientCode = p.PatientCode,
                PrescriptionsCount = p.Prescriptions.Count(),
                DispensingCount = p.DispensingRecords.Count()
            })
            .OrderByDescending(p => p.DispensingCount + p.PrescriptionsCount)
            .Take(10)
            .ToListAsync();

        model.TopPatients = patientActivity;

        return View(model);
    }
}
