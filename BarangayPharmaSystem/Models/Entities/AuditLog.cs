using System.ComponentModel.DataAnnotations;

namespace BarangayPharmaSystem.Models.Entities;

/// <summary>
/// Append-only audit trail. Records every significant action performed in the system.
/// Never deleted — this table is read-only after insert.
/// </summary>
public class AuditLog
{
    public int Id { get; set; }

    /// <summary>FK to the user who performed the action. Nullable for system-generated entries.</summary>
    public string? UserId { get; set; }

    /// <summary>Human-readable action description (e.g., "Created", "Updated", "Deleted", "Dispensed").</summary>
    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    /// <summary>Name of the database table/entity affected (e.g., "Patients", "Medicines").</summary>
    [Required]
    [MaxLength(100)]
    public string TableAffected { get; set; } = string.Empty;

    /// <summary>Primary key of the record affected. Stored as string to support both int and GUID PKs.</summary>
    [MaxLength(100)]
    public string? RecordId { get; set; }

    /// <summary>Optional additional context or changed values (JSON or plain text).</summary>
    [MaxLength(1000)]
    public string? Details { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>IP address of the request that triggered this log entry.</summary>
    [MaxLength(45)]
    public string? IPAddress { get; set; }

    // ── Navigation properties ────────────────────────────────────────────────

    public ApplicationUser? User { get; set; }
}
