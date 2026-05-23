using BarangayPharmaSystem.Models.Entities;

namespace BarangayPharmaSystem.Areas.Admin.Models;

public class AdminDashboardViewModel
{
    public int TotalPatients { get; set; }
    public int TotalMedicines { get; set; }
    public int TotalPrescriptions { get; set; }
    public int TotalDispensingRecords { get; set; }

    public List<StockAlert> ActiveAlerts { get; set; } = new();
    public List<AuditLog> RecentActivity { get; set; } = new();
}
