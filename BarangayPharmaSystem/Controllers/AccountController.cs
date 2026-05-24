using BarangayPharmaSystem.Data;
using BarangayPharmaSystem.Models.Entities;
using BarangayPharmaSystem.Models.ViewModels;
using BarangayPharmaSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BarangayPharmaSystem.Controllers;

/// <summary>
/// Handles user authentication: Login, Registration (Patient only), and Logout.
/// </summary>
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly AppDbContext _db;
    private readonly IAuditService _auditService;
    private readonly IFileUploadService _fileUploadService;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        AppDbContext db,
        IAuditService auditService,
        IFileUploadService fileUploadService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
        _auditService = auditService;
        _fileUploadService = fileUploadService;
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        // If already logged in, redirect based on role
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToRoleDashboard();
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        ViewData["ReturnUrl"] = model.ReturnUrl;

        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || user.IsDeleted)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your credentials.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            await _auditService.LogAsync("Login", "Users", user.Id, "User logged in successfully.");

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return LocalRedirect(model.ReturnUrl);
            }

            return RedirectToRoleDashboard(user);
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Account is locked out. Please try again later.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your credentials.");
        return View(model);
    }

    // ── Register (Patients Only) ─────────────────────────────────────────────

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToRoleDashboard();
        }

        return View(new RegisterViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        // Extra server-side date validation (defence-in-depth beyond ViewModel attribute)
        if (model.Birthdate.Date > DateTime.Today)
        {
            ModelState.AddModelError(nameof(model.Birthdate), "Birthdate cannot be in the future.");
        }

        if (!ModelState.IsValid)
            return View(model);

        // 1. Verify the PatientCode exists and is not already linked
        var patientProfile = await _db.Patients
            .FirstOrDefaultAsync(p => p.PatientCode == model.PatientCode && !p.IsDeleted);

        if (patientProfile == null || !string.IsNullOrEmpty(patientProfile.LinkedUserId))
        {
            ModelState.AddModelError(nameof(model.PatientCode), "Patient ID not found or already registered. Please visit the health center.");
            return View(model);
        }

        // 2. Create the ApplicationUser
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            PhoneNumber = model.ContactNumber,
            CreatedAt = DateTime.UtcNow,
            EmailConfirmed = true // Auto-confirm for this project
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            // 3. Assign Role
            await _userManager.AddToRoleAsync(user, "Patient");

            // 4. Link the existing Patient profile to this new User
            patientProfile.LinkedUserId = user.Id;
            // Optionally sync some fields if they differ, or rely on the profile for clinical data

            // 5. Handle Profile Photo Upload
            if (model.ProfilePhoto != null)
            {
                var uploadResult = await _fileUploadService.UploadUserPhotoAsync(model.ProfilePhoto, user.Id);
                if (uploadResult.Success)
                {
                    user.ProfilePhotoPath = uploadResult.RelativePath;
                    await _userManager.UpdateAsync(user); // Save the photo path
                }
                else
                {
                    ModelState.AddModelError(nameof(model.ProfilePhoto), uploadResult.ErrorMessage ?? "Photo upload failed.");
                    // Continue anyway, it's just a photo
                }
            }

            await _db.SaveChangesAsync();

            await _auditService.LogAsync("Registered", "Users", user.Id, $"Patient self-registered with code {model.PatientCode}.");

            TempData["SuccessMessage"] = "Registration successful! You can now log in.";
            return RedirectToAction(nameof(Login));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    // ── Logout ────────────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = _userManager.GetUserId(User);
        await _signInManager.SignOutAsync();
        
        if (userId != null)
        {
            await _auditService.LogAsync("Logout", "Users", userId, "User logged out.");
        }

        return RedirectToAction(nameof(Login));
    }

    // ── Access Denied ─────────────────────────────────────────────────────────

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private IActionResult RedirectToRoleDashboard(ApplicationUser? user = null)
    {
        // For convenience if the user object wasn't passed, check the claims
        if (User.IsInRole("Admin")) return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        if (User.IsInRole("Staff")) return RedirectToAction("Index", "Dashboard", new { area = "Staff" });
        if (User.IsInRole("Patient")) return RedirectToAction("Index", "Dashboard", new { area = "Patient" });

        // Fallback to the default dashboard if area is not used yet, or just generic dashboard
        return RedirectToAction("Index", "Dashboard");
    }
}
