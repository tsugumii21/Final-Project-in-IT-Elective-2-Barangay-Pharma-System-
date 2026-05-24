using System.ComponentModel.DataAnnotations;
using BarangayPharmaSystem.Models.Validation;

namespace BarangayPharmaSystem.Models.ViewModels;

/// <summary>
/// View model for the Patient self-registration form.
/// Patients must provide a valid PatientCode issued by staff before they can register.
/// </summary>
public class RegisterViewModel
{
    [Required(ErrorMessage = "Full name is required.")]
    [MaxLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [MaxLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    [RegularExpression(
        @"^(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$",
        ErrorMessage = "Password must include at least one uppercase letter, one number, and one special character.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Birthdate is required.")]
    [NotFutureDate(ErrorMessage = "Birthdate cannot be in the future.")]
    [DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    public DateTime Birthdate { get; set; } = DateTime.Today.AddYears(-18);

    [MaxLength(11, ErrorMessage = "Contact number must be 11 digits (e.g., 09XXXXXXXXX).")]
    [RegularExpression(@"^09\d{9}$", ErrorMessage = "Contact number must be in Philippine format (09XXXXXXXXX).")]
    [Display(Name = "Contact Number (09XXXXXXXXX)")]
    public string? ContactNumber { get; set; }

    /// <summary>
    /// The unique patient code (PAT-YYYY-NNNNN format) issued by barangay staff.
    /// Must match an existing, unlinked patient record in the database.
    /// </summary>
    [Required(ErrorMessage = "Patient ID is required to register.")]
    [RegularExpression(@"^PAT-\d{4}-\d{5}$",
        ErrorMessage = "Patient ID must be in the format PAT-YYYY-NNNNN (e.g., PAT-2026-00001).")]
    [Display(Name = "Patient ID (PAT-YYYY-NNNNN)")]
    public string PatientCode { get; set; } = string.Empty;

    /// <summary>Optional profile photo. Validated for type and size in the controller.</summary>
    [Display(Name = "Profile Photo (optional)")]
    public IFormFile? ProfilePhoto { get; set; }
}
