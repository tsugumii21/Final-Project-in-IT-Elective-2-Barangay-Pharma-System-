using Microsoft.AspNetCore.Mvc;

namespace BarangayPharmaSystem.Controllers;

/// <summary>Account controller — stub for Part 1. Full auth implementation in Part 2.</summary>
public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        ViewData["Title"] = "Sign In";
        return View();
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        ViewData["Title"] = "Access Denied";
        return View();
    }
}
