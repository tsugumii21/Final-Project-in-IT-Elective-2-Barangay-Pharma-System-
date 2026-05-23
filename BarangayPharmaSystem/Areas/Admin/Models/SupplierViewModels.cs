using System.ComponentModel.DataAnnotations;

namespace BarangayPharmaSystem.Areas.Admin.Models;

public class SupplierListViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? ContactNumber { get; set; }
}

public class SupplierFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(150)]
    [Display(Name = "Contact Person")]
    public string? ContactPerson { get; set; }

    [Phone, StringLength(20)]
    [Display(Name = "Contact Number")]
    public string? ContactNumber { get; set; }

    [StringLength(300)]
    public string? Address { get; set; }
}
