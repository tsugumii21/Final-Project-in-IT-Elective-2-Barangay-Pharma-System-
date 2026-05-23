using Microsoft.AspNetCore.Identity;

namespace BarangayPharmaSystem.Models.Entities;

/// <summary>
/// Extends ASP.NET Core Identity's IdentityUser with application-specific fields.
/// Mapped to the "Users" table in the database.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>Full display name of the user.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Relative path to the user's profile photo stored under wwwroot/uploads/users/.
    /// Null if no photo has been uploaded.
    /// </summary>
    public string? ProfilePhotoPath { get; set; }

    /// <summary>Soft-delete flag. Soft-deleted users cannot log in.</summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>Date/time this record was created (UTC).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation properties ────────────────────────────────────────────────

    /// <summary>Patient profile linked to this user account (if Role = Patient).</summary>
    public Patient? LinkedPatient { get; set; }

    /// <summary>Prescriptions written by this user (if Role = Staff/Admin).</summary>
    public ICollection<Prescription> WrittenPrescriptions { get; set; } = new List<Prescription>();

    /// <summary>Dispensing records processed by this user.</summary>
    public ICollection<DispensingRecord> DispensingRecords { get; set; } = new List<DispensingRecord>();

    /// <summary>Audit log entries attributed to this user.</summary>
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
