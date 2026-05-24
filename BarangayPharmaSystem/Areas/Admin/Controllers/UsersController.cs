using BarangayPharmaSystem.Areas.Admin.Models;
using BarangayPharmaSystem.Data;
using BarangayPharmaSystem.Models.Entities;
using BarangayPharmaSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BarangayPharmaSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _db;
    private readonly IAuditService _auditService;
    private readonly IFileUploadService _fileUploadService;

    public UsersController(
        UserManager<ApplicationUser> userManager,
        AppDbContext db,
        IAuditService auditService,
        IFileUploadService fileUploadService)
    {
        _userManager = userManager;
        _db = db;
        _auditService = auditService;
        _fileUploadService = fileUploadService;
    }

    // ── Index ─────────────────────────────────────────────────────────────────
    public async Task<IActionResult> Index(string? search)
    {
        var query = _userManager.Users.Where(u => !u.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim().ToLower();
            query = query.Where(u => (u.FullName != null && u.FullName.ToLower().Contains(search)) || 
                                     (u.Email != null && u.Email.ToLower().Contains(search)));
        }

        var users = await query.OrderBy(u => u.FullName).ToListAsync();
        
        var model = new List<UserListViewModel>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            model.Add(new UserListViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                Role = roles.FirstOrDefault() ?? "None",
                ProfilePhotoPath = user.ProfilePhotoPath,
                IsDeleted = user.IsDeleted
            });
        }

        ViewData["SearchQuery"] = search;
        return View(model);
    }

    // ── Create ────────────────────────────────────────────────────────────────
    public IActionResult Create()
    {
        return View(new UserCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserCreateViewModel model)
    {
        if (model.Role == "Patient" && !model.Birthdate.HasValue)
        {
            ModelState.AddModelError("Birthdate", "Birthdate is required for Patients.");
        }

        if (!ModelState.IsValid)
            return View(model);

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            PhoneNumber = model.ContactNumber,
            CreatedAt = DateTime.UtcNow,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, model.Role);

            // Handle Photo Upload
            if (model.ProfilePhoto != null)
            {
                var uploadResult = await _fileUploadService.UploadUserPhotoAsync(model.ProfilePhoto, user.Id);
                if (uploadResult.Success)
                {
                    user.ProfilePhotoPath = uploadResult.RelativePath;
                    await _userManager.UpdateAsync(user);
                }
            }

            string extraDetails = "";

            // Handle Patient Profile Generation
            if (model.Role == "Patient")
            {
                // Generate PatientCode PAT-YYYY-NNNNN
                int currentYear = DateTime.UtcNow.Year;
                var lastPatient = await _db.Patients
                    .Where(p => p.PatientCode.StartsWith($"PAT-{currentYear}-"))
                    .OrderByDescending(p => p.Id)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (lastPatient != null)
                {
                    var parts = lastPatient.PatientCode.Split('-');
                    if (parts.Length == 3 && int.TryParse(parts[2], out int lastNum))
                    {
                        nextNumber = lastNum + 1;
                    }
                }

                string patientCode = $"PAT-{currentYear}-{nextNumber:D5}";

                var patient = new BarangayPharmaSystem.Models.Entities.Patient
                {
                    PatientCode = patientCode,
                    FullName = model.FullName,
                    Birthdate = model.Birthdate!.Value,
                    ContactNumber = model.ContactNumber ?? "",
                    Address = model.Address ?? "",
                    LinkedUserId = user.Id,
                    ProfilePhotoPath = user.ProfilePhotoPath,
                    CreatedAt = DateTime.UtcNow
                };

                _db.Patients.Add(patient);
                await _db.SaveChangesAsync();
                
                extraDetails = $" Patient Code: {patientCode}.";
                TempData["PatientCodeAlert"] = patientCode; // Show code to nurse
            }

            await _auditService.LogAsync("Created", "Users", user.Id, $"Admin created user '{user.FullName}' ({user.Email}) with role '{model.Role}'.");

            TempData["SuccessMessage"] = $"User {user.FullName} created successfully!";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    // ── Edit ──────────────────────────────────────────────────────────────────
    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();

        var user = await _userManager.FindByIdAsync(id);
        if (user == null || user.IsDeleted) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Staff";

        var model = new UserEditViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? "",
            ContactNumber = user.PhoneNumber,
            Role = role,
            CurrentPhotoPath = user.ProfilePhotoPath
        };

        if (role == "Patient")
        {
            var patient = await _db.Patients.FirstOrDefaultAsync(p => p.LinkedUserId == user.Id);
            if (patient != null)
            {
                model.PatientCode = patient.PatientCode;
                model.Birthdate = patient.Birthdate;
                model.Address = patient.Address;
            }
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserEditViewModel model)
    {
        if (model.Role == "Patient" && !model.Birthdate.HasValue)
        {
            ModelState.AddModelError("Birthdate", "Birthdate is required for Patients.");
        }

        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByIdAsync(model.Id);
        if (user == null || user.IsDeleted) return NotFound();

        user.FullName = model.FullName;
        user.Email = model.Email;
        user.UserName = model.Email;
        user.PhoneNumber = model.ContactNumber;

        if (model.ProfilePhoto != null)
        {
            var uploadResult = await _fileUploadService.UploadUserPhotoAsync(model.ProfilePhoto, user.Id);
            if (uploadResult.Success)
            {
                if (!string.IsNullOrEmpty(user.ProfilePhotoPath))
                {
                    _fileUploadService.DeleteFile(user.ProfilePhotoPath);
                }
                user.ProfilePhotoPath = uploadResult.RelativePath;
            }
        }

        await _userManager.UpdateAsync(user);

        // Manage Role changes
        var currentRoles = await _userManager.GetRolesAsync(user);
        if (!currentRoles.Contains(model.Role))
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, model.Role);
        }

        // Manage Patient Profile
        if (model.Role == "Patient")
        {
            var patient = await _db.Patients.FirstOrDefaultAsync(p => p.LinkedUserId == user.Id);
            if (patient != null)
            {
                patient.FullName = model.FullName;
                patient.ContactNumber = model.ContactNumber ?? "";
                if (model.Birthdate.HasValue) patient.Birthdate = model.Birthdate.Value;
                patient.Address = model.Address ?? "";
                patient.ProfilePhotoPath = user.ProfilePhotoPath;
                _db.Patients.Update(patient);
            }
            else
            {
                // Edge case: Role was changed to Patient
                // Need to generate code
                int currentYear = DateTime.UtcNow.Year;
                var lastPatient = await _db.Patients
                    .Where(p => p.PatientCode.StartsWith($"PAT-{currentYear}-"))
                    .OrderByDescending(p => p.Id)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (lastPatient != null)
                {
                    var parts = lastPatient.PatientCode.Split('-');
                    if (parts.Length == 3 && int.TryParse(parts[2], out int lastNum))
                    {
                        nextNumber = lastNum + 1;
                    }
                }

                string patientCode = $"PAT-{currentYear}-{nextNumber:D5}";

                var newPatient = new BarangayPharmaSystem.Models.Entities.Patient
                {
                    PatientCode = patientCode,
                    FullName = model.FullName,
                    Birthdate = model.Birthdate ?? DateTime.Today,
                    ContactNumber = model.ContactNumber ?? "",
                    Address = model.Address ?? "",
                    LinkedUserId = user.Id,
                    ProfilePhotoPath = user.ProfilePhotoPath,
                    CreatedAt = DateTime.UtcNow
                };
                _db.Patients.Add(newPatient);
                TempData["PatientCodeAlert"] = patientCode;
            }
            await _db.SaveChangesAsync();
        }

        var adminId = _userManager.GetUserId(User);
        await _auditService.LogAsync("Updated", "Users", user.Id, $"Admin updated user '{user.FullName}'.");

        TempData["SuccessMessage"] = "User updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    // ── Delete (Soft) ─────────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null || user.IsDeleted) return NotFound();

        // Prevent admin from deleting themselves
        if (user.Id == _userManager.GetUserId(User))
        {
            TempData["ErrorMessage"] = "You cannot delete your own account.";
            return RedirectToAction(nameof(Index));
        }

        user.IsDeleted = true;
        await _userManager.UpdateAsync(user);

        // Also soft delete patient profile if exists
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.LinkedUserId == user.Id);
        if (patient != null)
        {
            patient.IsDeleted = true;
            _db.Patients.Update(patient);
            await _db.SaveChangesAsync();
        }

        var adminId = _userManager.GetUserId(User);
        await _auditService.LogAsync("Deleted", "Users", user.Id, $"Admin deleted user '{user.FullName}'.");

        TempData["SuccessMessage"] = "User deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
