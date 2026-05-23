using System.ComponentModel.DataAnnotations;

namespace BarangayPharmaSystem.Models.ViewModels;

/// <summary>View model for the Login form.</summary>
public class LoginViewModel
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; } = false;

    /// <summary>URL to return to after successful login (set by ASP.NET Core Identity middleware).</summary>
    public string? ReturnUrl { get; set; }
}
