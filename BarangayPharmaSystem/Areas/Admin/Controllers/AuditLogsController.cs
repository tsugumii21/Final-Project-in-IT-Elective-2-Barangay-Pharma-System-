using BarangayPharmaSystem.Data;
using BarangayPharmaSystem.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BarangayPharmaSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AuditLogsController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuditLogsController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? userId, string? actionType, DateTime? startDate, DateTime? endDate)
    {
        var query = _db.AuditLogs.Include(a => a.User).AsQueryable();

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(a => a.UserId == userId);
        }

        if (!string.IsNullOrEmpty(actionType))
        {
            query = query.Where(a => a.Action == actionType);
        }

        if (startDate.HasValue)
        {
            query = query.Where(a => a.Timestamp >= startDate.Value.ToUniversalTime());
        }

        if (endDate.HasValue)
        {
            // Include the entire end date
            var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
            query = query.Where(a => a.Timestamp <= endOfDay);
        }

        var logs = await query.OrderByDescending(a => a.Timestamp).Take(500).ToListAsync();

        // Populate dropdowns
        var users = await _userManager.Users.OrderBy(u => u.FullName).ToListAsync();
        ViewBag.UsersList = users.Select(u => new SelectListItem { Value = u.Id, Text = $"{u.FullName} ({u.Email})" });

        var actionTypes = await _db.AuditLogs.Select(a => a.Action).Distinct().ToListAsync();
        ViewBag.ActionTypes = actionTypes.Select(a => new SelectListItem { Value = a, Text = a });

        // Keep filter values
        ViewData["UserId"] = userId;
        ViewData["ActionType"] = actionType;
        ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
        ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");

        return View(logs);
    }
}
