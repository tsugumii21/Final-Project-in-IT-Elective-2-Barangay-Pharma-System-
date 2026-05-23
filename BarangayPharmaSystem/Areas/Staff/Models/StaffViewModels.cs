using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using BarangayPharmaSystem.Models.Entities;

namespace BarangayPharmaSystem.Areas.Staff.Models;

// ── Dashboard ─────────────────────────────────────────────────────────────
public class StaffDashboardViewModel
{
    public int MyPatientsToday { get; set; }
    public int PendingRefillRequests { get; set; }
    public int LowStockAlerts { get; set; }

    public List<DispensingRecord> TodaysDispensing { get; set; } = new();
}

// ── Patient Management ────────────────────────────────────────────────────
public class StaffPatientListViewModel
{
    public int Id { get; set; }
    public string PatientCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? ContactNumber { get; set; }
    public int Age { get; set; }
    public string? ProfilePhotoPath { get; set; }
    public bool HasLinkedAccount { get; set; }
}

public class StaffPatientFormViewModel
{
    public int Id { get; set; }

    public string? PatientCode { get; set; } // Auto-generated for Create

    [Required]
    [MaxLength(150)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Birthdate")]
    public DateTime Birthdate { get; set; } = DateTime.Today.AddYears(-20);

    [Required]
    [MaxLength(300)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(20)]
    [Display(Name = "Contact Number")]
    public string? ContactNumber { get; set; }

    [Display(Name = "Profile Photo")]
    public IFormFile? PhotoFile { get; set; }

    public string? CurrentPhotoPath { get; set; }
}

public class StaffPatientDetailsViewModel
{
    public Patient Patient { get; set; } = null!;
    public List<Prescription> Prescriptions { get; set; } = new();
    public List<DispensingRecord> DispensingHistory { get; set; } = new();
}

// ── Prescription Management ───────────────────────────────────────────────
public class StaffPrescriptionListViewModel
{
    public int Id { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string PatientCode { get; set; } = string.Empty;
    public string MedicineName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public PrescriptionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class StaffPrescriptionFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Patient is required.")]
    [Display(Name = "Patient")]
    public int PatientId { get; set; }

    [Required(ErrorMessage = "Medicine is required.")]
    [Display(Name = "Medicine")]
    public int MedicineId { get; set; }

    [Required]
    [MaxLength(150)]
    [Display(Name = "Doctor Name")]
    public string DoctorName { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    [Display(Name = "Dosage (e.g. 500mg - 3x daily)")]
    public string Dosage { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Display(Name = "Duration (e.g. 7 days)")]
    public string Duration { get; set; } = string.Empty;
    
    [Display(Name = "Status")]
    public PrescriptionStatus Status { get; set; } = PrescriptionStatus.Active;
}

// ── Medicine Dispensing ───────────────────────────────────────────────────
public class StaffDispensingListViewModel
{
    public int Id { get; set; }
    public DateTime DateDispensed { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string MedicineName { get; set; } = string.Empty;
    public int QuantityDispensed { get; set; }
    public string StaffName { get; set; } = string.Empty;
}

public class StaffDispensingFormViewModel
{
    [Required]
    [Display(Name = "Select Active Prescription")]
    public int PrescriptionId { get; set; }

    [Required]
    [Range(1, 1000, ErrorMessage = "Quantity must be greater than zero.")]
    [Display(Name = "Quantity to Dispense")]
    public int Quantity { get; set; }

    [MaxLength(500)]
    [Display(Name = "Staff Notes (Optional)")]
    public string? Notes { get; set; }
}

// ── Refill Requests ───────────────────────────────────────────────────────
public class StaffRefillRequestListViewModel
{
    public int Id { get; set; }
    public DateTime RequestDate { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string MedicineName { get; set; } = string.Empty;
    public string PrescriptionDosage { get; set; } = string.Empty;
    public string? PatientNotes { get; set; }
    public RefillRequestStatus Status { get; set; }
    public int CurrentStock { get; set; }
    public int PrescriptionId { get; set; }
}

// ── Inventory ─────────────────────────────────────────────────────────────
public class StaffInventoryViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Stock { get; set; }
    public int MinStockLevel { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string? PhotoPath { get; set; }
}
