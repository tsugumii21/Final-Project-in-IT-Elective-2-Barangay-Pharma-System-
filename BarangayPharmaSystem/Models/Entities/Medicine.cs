using System.ComponentModel.DataAnnotations;

namespace BarangayPharmaSystem.Models.Entities;

/// <summary>
/// Represents a medicine/drug stocked in the barangay pharmacy.
/// Stock changes are tracked and trigger alerts when below MinStockLevel or near expiry.
/// </summary>
public class Medicine
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    /// <summary>Current stock quantity (units).</summary>
    [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
    public int Stock { get; set; } = 0;

    /// <summary>
    /// Threshold below which a LowStock alert is automatically generated.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int MinStockLevel { get; set; } = 10;

    [Required]
    public DateTime ExpiryDate { get; set; }

    /// <summary>Dosage instructions (e.g., "500mg — Take twice daily with meals").</summary>
    [MaxLength(500)]
    public string? DosageInfo { get; set; }

    /// <summary>Relative path to the medicine's photo under wwwroot/uploads/medicines/.</summary>
    public string? PhotoPath { get; set; }

    /// <summary>Soft-delete flag.</summary>
    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation properties ────────────────────────────────────────────────

    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    public ICollection<StockAlert> StockAlerts { get; set; } = new List<StockAlert>();

    // ── Computed helpers ─────────────────────────────────────────────────────

    /// <summary>Returns true if stock is at or below the minimum stock level.</summary>
    public bool IsLowStock => Stock <= MinStockLevel;

    /// <summary>Returns true if the medicine expires within 30 days.</summary>
    public bool IsNearExpiry => ExpiryDate <= DateTime.Today.AddDays(30);

    /// <summary>Returns true if the medicine has already expired.</summary>
    public bool IsExpired => ExpiryDate < DateTime.Today;
}
