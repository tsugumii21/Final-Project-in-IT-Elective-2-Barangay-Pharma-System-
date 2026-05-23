using BarangayPharmaSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarangayPharmaSystem.Controllers;

/// <summary>Default landing controller — redirects authenticated users to Dashboard.</summary>
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
}
