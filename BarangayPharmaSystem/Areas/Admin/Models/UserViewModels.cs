using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

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
    [Required, StringLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Contact Number")]
    public string? ContactNumber { get; set; }

    [Required, DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Staff";

    [Display(Name = "Profile Photo")]
    public IFormFile? ProfilePhoto { get; set; }

    // Patient specific fields
    [DataType(DataType.Date)]
    public DateTime? Birthdate { get; set; }
    public string? Address { get; set; }
}

public class UserEditViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required, StringLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Contact Number")]
    public string? ContactNumber { get; set; }

    [Required]
    public string Role { get; set; } = string.Empty;

    public string? CurrentPhotoPath { get; set; }

    [Display(Name = "New Profile Photo (Optional)")]
    public IFormFile? ProfilePhoto { get; set; }

    // Patient specific fields
    public string? PatientCode { get; set; }
    [DataType(DataType.Date)]
    public DateTime? Birthdate { get; set; }
    public string? Address { get; set; }
}
