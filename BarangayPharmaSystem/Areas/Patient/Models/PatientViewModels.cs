using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using BarangayPharmaSystem.Models.Entities;

namespace BarangayPharmaSystem.Areas.Patient.Models;

// ── Dashboard ─────────────────────────────────────────────────────────────
public class PatientDashboardViewModel
{
    public string PatientName { get; set; } = string.Empty;
    public string? ProfilePhotoPath { get; set; }
    
    public int ActivePrescriptions { get; set; }
    public int TotalDispensingRecords { get; set; }
    public int PendingRefillRequests { get; set; }

    public Prescription? LatestPrescription { get; set; }
}

// ── Profile ───────────────────────────────────────────────────────────────
public class PatientProfileViewModel
{
    public int Id { get; set; }
    public string PatientCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime Birthdate { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? ContactNumber { get; set; }
    public string? CurrentPhotoPath { get; set; }

    [Display(Name = "Update Profile Photo")]
    public IFormFile? PhotoFile { get; set; }
}

// ── Prescriptions ─────────────────────────────────────────────────────────
public class PatientPrescriptionListViewModel
{
    public int Id { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public PrescriptionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // For Refill Cooldown Logic
    public bool CanRequestRefill { get; set; }
    public string? RefillBlockReason { get; set; }
}

// ── Dispensing History ────────────────────────────────────────────────────
public class PatientDispensingListViewModel
{
    public int Id { get; set; }
    public DateTime DateDispensed { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public int QuantityDispensed { get; set; }
    public string StaffName { get; set; } = string.Empty;
}

// ── Refill Requests ───────────────────────────────────────────────────────
public class PatientRefillRequestListViewModel
{
    public int Id { get; set; }
    public DateTime RequestDate { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public RefillRequestStatus Status { get; set; }
}

public class PatientRefillRequestFormViewModel
{
    [Required]
    public int PrescriptionId { get; set; }
    
    public string MedicineName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;

    [MaxLength(500)]
    [Display(Name = "Additional Notes / Reason for Refill")]
    public string? Notes { get; set; }
}
