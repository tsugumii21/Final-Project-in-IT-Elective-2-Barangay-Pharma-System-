using System.ComponentModel.DataAnnotations;

namespace BarangayPharmaSystem.Models.Entities;

/// <summary>
/// Represents a patient registered in the barangay pharmacy system.
/// Patients are linked to an ApplicationUser account via LinkedUserId.
/// </summary>
public class Patient
{
    public int Id { get; set; }

    /// <summary>
    /// Unique patient identifier in PAT-YYYY-NNNN format.
    /// Generated automatically on creation. Used for patient self-registration.
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string PatientCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public DateTime Birthdate { get; set; }

    [Required]
    [MaxLength(300)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? ContactNumber { get; set; }

    /// <summary>Relative path to the patient's profile photo under wwwroot/uploads/patients/.</summary>
    public string? ProfilePhotoPath { get; set; }

    /// <summary>FK to ApplicationUser — null until the patient creates their login account.</summary>
    public string? LinkedUserId { get; set; }

    /// <summary>Soft-delete flag. Soft-deleted patients are hidden from all lists.</summary>
    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation properties ────────────────────────────────────────────────

    /// <summary>The user account linked to this patient record.</summary>
    public ApplicationUser? LinkedUser { get; set; }

    /// <summary>All prescriptions issued to this patient.</summary>
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    /// <summary>All dispensing records for this patient.</summary>
    public ICollection<DispensingRecord> DispensingRecords { get; set; } = new List<DispensingRecord>();

    /// <summary>All refill requests submitted by this patient.</summary>
    public ICollection<RefillRequest> RefillRequests { get; set; } = new List<RefillRequest>();

    // ── Computed helpers ─────────────────────────────────────────────────────

    /// <summary>Calculates the patient's age from their birthdate.</summary>
    public int Age => (int)((DateTime.Today - Birthdate).TotalDays / 365.25);
}
