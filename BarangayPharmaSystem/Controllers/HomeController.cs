using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarangayPharmaSystem.Controllers;

/// <summary>Default landing controller — redirects authenticated users to Dashboard.
/// Also serves public pages: SDG3 and About.</summary>
public class HomeController : Controller
{
    [AllowAnonymous]
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return RedirectToAction("Login", "Account");
    }

    [AllowAnonymous]
    public IActionResult SDG3()
    {
        ViewData["Title"] = "SDG 3 — Good Health and Well-being";
        return View();
    }

    [AllowAnonymous]
    public IActionResult About()
    {
        ViewData["Title"] = "About — Barangay Pharma System";
        return View();
    }
}
