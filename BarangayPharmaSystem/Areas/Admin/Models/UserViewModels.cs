using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using BarangayPharmaSystem.Models.Validation;

namespace BarangayPharmaSystem.Areas.Admin.Models;

public class UserListViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? ProfilePhotoPath { get; set; }
    public bool IsDeleted { get; set; }
}

public class UserCreateViewModel
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
    public string Email { get; set; } = string.Empty;

    [MaxLength(11, ErrorMessage = "Contact number must be 11 digits.")]
    [RegularExpression(@"^09\d{9}$", ErrorMessage = "Contact number must be in Philippine format (09XXXXXXXXX).")]
    [Display(Name = "Contact Number (09XXXXXXXXX)")]
    public string? ContactNumber { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
    [RegularExpression(
        @"^(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$",
        ErrorMessage = "Password must include at least one uppercase letter, one number, and one special character.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required.")]
    public string Role { get; set; } = "Staff";

    [Display(Name = "Profile Photo")]
    public IFormFile? ProfilePhoto { get; set; }

    // Patient-specific fields (only required when Role == "Patient")
    [NotFutureDate(ErrorMessage = "Birthdate cannot be in the future.")]
    [DataType(DataType.Date)]
    public DateTime? Birthdate { get; set; }

    [StringLength(300, ErrorMessage = "Address cannot exceed 300 characters.")]
    public string? Address { get; set; }
}

public class UserEditViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
    public string Email { get; set; } = string.Empty;

    [MaxLength(11, ErrorMessage = "Contact number must be 11 digits.")]
    [RegularExpression(@"^09\d{9}$", ErrorMessage = "Contact number must be in Philippine format (09XXXXXXXXX).")]
    [Display(Name = "Contact Number (09XXXXXXXXX)")]
    public string? ContactNumber { get; set; }

    [Required(ErrorMessage = "Role is required.")]
    public string Role { get; set; } = string.Empty;

    public string? CurrentPhotoPath { get; set; }

    [Display(Name = "New Profile Photo (Optional)")]
    public IFormFile? ProfilePhoto { get; set; }

    // Patient-specific fields
    public string? PatientCode { get; set; }

    [NotFutureDate(ErrorMessage = "Birthdate cannot be in the future.")]
    [DataType(DataType.Date)]
    public DateTime? Birthdate { get; set; }

    [StringLength(300, ErrorMessage = "Address cannot exceed 300 characters.")]
    public string? Address { get; set; }
}
