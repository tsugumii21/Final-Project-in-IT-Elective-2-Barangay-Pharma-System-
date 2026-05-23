using System.ComponentModel.DataAnnotations;

namespace BarangayPharmaSystem.Models.Entities;

/// <summary>Possible states of a refill request.</summary>
public enum RefillRequestStatus
{
    /// <summary>Request submitted and awaiting staff review.</summary>
    Pending = 0,

    /// <summary>Request approved — prescription refill is authorised.</summary>
    Approved = 1,

    /// <summary>Request rejected by staff (e.g., too early, no stock, clinical reason).</summary>
    Rejected = 2
}

/// <summary>
/// Represents a patient's request to refill an existing prescription.
/// Includes cooldown enforcement to prevent spamming.
/// </summary>
public class RefillRequest
{
    public int Id { get; set; }

    // ── Foreign keys ─────────────────────────────────────────────────────────

    [Required]
    public int PrescriptionId { get; set; }

    [Required]
    public int PatientId { get; set; }

    // ── Request details ──────────────────────────────────────────────────────

    public DateTime RequestDate { get; set; } = DateTime.UtcNow;

    public RefillRequestStatus Status { get; set; } = RefillRequestStatus.Pending;

    [MaxLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// The earliest date/time the patient can submit another refill request
    /// for this prescription. Null if no cooldown is active.
    /// </summary>
    public DateTime? CooldownUntil { get; set; }

    // ── Navigation properties ────────────────────────────────────────────────

    public Prescription Prescription { get; set; } = null!;
    public Patient Patient { get; set; } = null!;

    // ── Computed helpers ─────────────────────────────────────────────────────

    /// <summary>Returns true if the patient is still within the cooldown period.</summary>
    public bool IsOnCooldown => CooldownUntil.HasValue && CooldownUntil.Value > DateTime.UtcNow;
}
