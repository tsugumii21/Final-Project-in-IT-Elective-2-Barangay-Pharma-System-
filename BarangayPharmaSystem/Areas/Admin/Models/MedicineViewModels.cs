using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using BarangayPharmaSystem.Models.Validation;

namespace BarangayPharmaSystem.Areas.Admin.Models;

public class MedicineListViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Stock { get; set; }
    public int MinStockLevel { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string? SupplierName { get; set; }
    public string? PhotoPath { get; set; }

    public bool IsLowStock   => Stock <= MinStockLevel;
    public bool IsNearExpiry => ExpiryDate <= DateTime.Today.AddDays(30);
    public bool IsExpired    => ExpiryDate < DateTime.Today;
    public bool IsOutOfStock => Stock <= 0;
}

public class MedicineFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Medicine name is required.")]
    [StringLength(100, ErrorMessage = "Medicine name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required.")]
    [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters.")]
    public string Category { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
    public int Stock { get; set; }

    [Display(Name = "Minimum Stock Level")]
    [Range(0, int.MaxValue, ErrorMessage = "Minimum stock level cannot be negative.")]
    public int MinStockLevel { get; set; } = 10;

    [Required(ErrorMessage = "Expiry date is required.")]
    [NotPastDate(ErrorMessage = "Expiry date cannot be in the past.")]
    [DataType(DataType.Date)]
    [Display(Name = "Expiry Date")]
    public DateTime ExpiryDate { get; set; }

    [Display(Name = "Dosage Info")]
    [StringLength(500, ErrorMessage = "Dosage info cannot exceed 500 characters.")]
    public string? DosageInfo { get; set; }

    [Display(Name = "Supplier")]
    public int? SupplierId { get; set; }
    public IEnumerable<SelectListItem>? SuppliersList { get; set; }

    public string? CurrentPhotoPath { get; set; }

    [Display(Name = "Medicine Photo")]
    public IFormFile? PhotoFile { get; set; }
}
