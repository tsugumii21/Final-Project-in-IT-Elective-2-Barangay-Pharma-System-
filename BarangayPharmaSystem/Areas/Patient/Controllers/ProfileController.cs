using BarangayPharmaSystem.Areas.Patient.Models;
using BarangayPharmaSystem.Data;
using BarangayPharmaSystem.Models.Entities;
using BarangayPharmaSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BarangayPharmaSystem.Areas.Patient.Controllers;

[Area("Patient")]
[Authorize(Roles = "Patient")]
public class ProfileController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFileUploadService _fileUploadService;

    public ProfileController(AppDbContext db, UserManager<ApplicationUser> userManager, IFileUploadService fileUploadService)
    {
        _db = db;
        _userManager = userManager;
        _fileUploadService = fileUploadService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.LinkedUserId == userId);

        if (patient == null) return RedirectToAction("Index", "Dashboard", new { area = "Patient" });

        var model = new PatientProfileViewModel
        {
            Id = patient.Id,
            PatientCode = patient.PatientCode,
            FullName = patient.FullName,
            Birthdate = patient.Birthdate,
            Address = patient.Address,
            ContactNumber = patient.ContactNumber,
            CurrentPhotoPath = patient.ProfilePhotoPath
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePhoto(PatientProfileViewModel model)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Unauthorized();

        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.LinkedUserId == userId);

        if (patient == null) return NotFound();

        if (model.PhotoFile != null)
        {
            var uploadResult = await _fileUploadService.UploadPatientPhotoAsync(model.PhotoFile, patient.Id);
            if (uploadResult.Success)
            {
                // Delete old photo
                if (!string.IsNullOrEmpty(patient.ProfilePhotoPath))
                {
                    _fileUploadService.DeleteFile(patient.ProfilePhotoPath);
                }

                patient.ProfilePhotoPath = uploadResult.RelativePath;
                _db.Patients.Update(patient);

                // Update ApplicationUser photo as well
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    user.ProfilePhotoPath = uploadResult.RelativePath;
                    await _userManager.UpdateAsync(user);
                }

                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Profile photo updated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = uploadResult.ErrorMessage ?? "Failed to upload photo.";
            }
        }

        return RedirectToAction(nameof(Index));
    }
}
