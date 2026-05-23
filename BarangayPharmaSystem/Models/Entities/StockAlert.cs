using System.ComponentModel.DataAnnotations;

namespace BarangayPharmaSystem.Models.Entities;

/// <summary>Types of stock alerts the system can generate.</summary>
public enum AlertType
{
    LowStock = 0,
    NearExpiry = 1,
    Expired = 2
}

/// <summary>
/// Represents a stock alert for a medicine — triggered when stock drops
/// below MinStockLevel or when ExpiryDate is within 30 days.
/// Alerts can be resolved (IsResolved = true) by Admin or Staff.
/// </summary>
public class StockAlert
{
    public int Id { get; set; }

    [Required]
    public int MedicineId { get; set; }

    public AlertType AlertType { get; set; }

    [Required]
    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    /// <summary>True once a staff member acknowledges and resolves the alert.</summary>
    public bool IsResolved { get; set; } = false;

    public DateTime? ResolvedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation properties ────────────────────────────────────────────────

    public Medicine Medicine { get; set; } = null!;
}
