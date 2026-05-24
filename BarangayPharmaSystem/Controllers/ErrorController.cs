using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BarangayPharmaSystem.Controllers;

/// <summary>
/// Handles custom error pages for HTTP status codes and unhandled exceptions.
/// All actions are AllowAnonymous so error pages are always accessible.
/// </summary>
[AllowAnonymous]
public class ErrorController : Controller
{
    private readonly ILogger<ErrorController> _logger;

    public ErrorController(ILogger<ErrorController> logger)
    {
        _logger = logger;
    }

    /// <summary>Handles 404 Not Found errors.</summary>
    [Route("/error/404")]
    public IActionResult NotFound404()
    {
        Response.StatusCode = 404;
        ViewData["Title"] = "Page Not Found";
        return View("NotFound");
    }

    /// <summary>Handles 403 Forbidden / Access Denied errors.</summary>
    [Route("/error/403")]
    public IActionResult Forbidden()
    {
        Response.StatusCode = 403;
        ViewData["Title"] = "Access Denied";
        return View("AccessDenied");
    }

    /// <summary>Handles 500 and other unhandled server errors.</summary>
    [Route("/error/500")]
    [Route("/error")]
    public IActionResult ServerError()
    {
        // Log the exception details if available
        var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        if (exceptionFeature != null)
        {
            _logger.LogError(exceptionFeature.Error,
                "Unhandled exception at path: {Path}", exceptionFeature.Path);
        }

        Response.StatusCode = 500;
        ViewData["Title"] = "Server Error";
        return View("ServerError");
    }
}
