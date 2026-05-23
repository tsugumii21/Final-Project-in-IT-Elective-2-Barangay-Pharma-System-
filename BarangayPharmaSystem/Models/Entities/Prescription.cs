using System.ComponentModel.DataAnnotations;

namespace BarangayPharmaSystem.Models.Entities;

/// <summary>Possible lifecycle states of a prescription.</summary>
public enum PrescriptionStatus
{
    Active = 0,
    Completed = 1,
    Cancelled = 2
}

/// <summary>
/// Represents a prescription linking a patient to a medicine, written by a staff member.
/// Stock is NOT deducted at prescription creation — only when a DispensingRecord is created.
/// </summary>
public class Prescription
{
    public int Id { get; set; }

    // ── Foreign keys ─────────────────────────────────────────────────────────

    [Required]
    public int PatientId { get; set; }

    /// <summary>FK to ApplicationUser (Staff/Admin who wrote the prescription).</summary>
    [Required]
    public string StaffId { get; set; } = string.Empty;

    [Required]
    public int MedicineId { get; set; }

    // ── Prescription details ─────────────────────────────────────────────────

    /// <summary>Name of the doctor who authorized the prescription.</summary>
    [Required]
    [MaxLength(150)]
    public string DoctorName { get; set; } = string.Empty;

    /// <summary>Prescribed dosage instructions (e.g., "500mg — twice daily").</summary>
    [Required]
    [MaxLength(300)]
    public string Dosage { get; set; } = string.Empty;

    /// <summary>Duration of treatment (e.g., "7 days", "1 month").</summary>
    [Required]
    [MaxLength(100)]
    public string Duration { get; set; } = string.Empty;

    public PrescriptionStatus Status { get; set; } = PrescriptionStatus.Active;

    /// <summary>Soft-delete flag.</summary>
    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation properties ────────────────────────────────────────────────

    public Patient Patient { get; set; } = null!;
    public ApplicationUser Staff { get; set; } = null!;
    public Medicine Medicine { get; set; } = null!;

    public ICollection<DispensingRecord> DispensingRecords { get; set; } = new List<DispensingRecord>();
    public ICollection<RefillRequest> RefillRequests { get; set; } = new List<RefillRequest>();
}
