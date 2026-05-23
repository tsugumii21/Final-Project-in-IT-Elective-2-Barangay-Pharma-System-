namespace BarangayPharmaSystem.Areas.Admin.Models;

public class ReportsViewModel
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public int TotalDispensedRecords { get; set; }
    public int TotalMedicinesDispensed { get; set; }

    public List<MedicineUsageReport> TopMedicines { get; set; } = new();
    public List<LowStockReport> LowStockMedicines { get; set; } = new();
    public List<PatientActivityReport> TopPatients { get; set; } = new();
}

public class MedicineUsageReport
{
    public string MedicineName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int TotalDispensed { get; set; }
    public int TimesDispensed { get; set; }
}

public class LowStockReport
{
    public string MedicineName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int MinStockLevel { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class PatientActivityReport
{
    public string PatientName { get; set; } = string.Empty;
    public string PatientCode { get; set; } = string.Empty;
    public int PrescriptionsCount { get; set; }
    public int DispensingCount { get; set; }
}
