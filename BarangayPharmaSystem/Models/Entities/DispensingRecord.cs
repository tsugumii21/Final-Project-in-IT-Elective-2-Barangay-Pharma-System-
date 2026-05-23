using System.ComponentModel.DataAnnotations;

namespace BarangayPharmaSystem.Models.Entities;

/// <summary>
/// Records a single dispensing event where medicine is physically given to a patient.
/// This is the trigger for stock deduction from Medicine.Stock.
/// </summary>
public class DispensingRecord
{
    public int Id { get; set; }

    // ── Foreign keys ─────────────────────────────────────────────────────────

    [Required]
    public int PrescriptionId { get; set; }

    [Required]
    public int PatientId { get; set; }

    /// <summary>FK to ApplicationUser (staff who performed the dispensing).</summary>
    [Required]
    public string StaffId { get; set; } = string.Empty;

    // ── Dispensing details ───────────────────────────────────────────────────

    /// <summary>
    /// Number of units dispensed. Deducted from Medicine.Stock when this record is saved.
    /// </summary>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity dispensed must be at least 1.")]
    public int QuantityDispensed { get; set; }

    public DateTime DateDispensed { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? Notes { get; set; }

    // ── Navigation properties ────────────────────────────────────────────────

    public Prescription Prescription { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public ApplicationUser Staff { get; set; } = null!;
}
