using System.ComponentModel.DataAnnotations;

namespace BarangayPharmaSystem.Models.Entities;

/// <summary>
/// Represents a medicine supplier or vendor used by the pharmacy.
/// </summary>
public class Supplier
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? ContactPerson { get; set; }

    [MaxLength(20)]
    public string? ContactNumber { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }

    /// <summary>Soft-delete flag.</summary>
    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
