using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarangayPharmaSystem.Controllers;

/// <summary>Dashboard controller — stub for Part 1. Full implementation in Part 3.</summary>
[Authorize]
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"]     = "Dashboard";
        ViewData["PageTitle"] = "Dashboard";
        return View();
    }
}
