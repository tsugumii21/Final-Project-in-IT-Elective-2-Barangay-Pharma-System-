using BarangayPharmaSystem.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BarangayPharmaSystem.Data;

/// <summary>
/// Seeds the database with required baseline data on application startup.
/// Idempotent — checks for existence before inserting any record.
/// Seeding order matters: Roles → Users → Patients → Suppliers → Medicines → Prescriptions → DispensingRecords.
/// </summary>
public static class DbSeeder
{
    // ── Role constants ───────────────────────────────────────────────────────
    private const string AdminRoleName   = "Admin";
    private const string StaffRoleName   = "Staff";
    private const string PatientRoleName = "Patient";

    // ── Seed user credentials ────────────────────────────────────────────────
    private const string AdminEmail    = "admin@bps.com";
    private const string AdminPassword = "Admin@1234";
    private const string AdminFullName = "System Administrator";

    private const string StaffEmail    = "staff@bps.com";
    private const string StaffPassword = "Staff@1234";
    private const string StaffFullName = "Maria Santos";

    private const string PatientEmail    = "patient@bps.com";
    private const string PatientPassword = "Patient@1234";
    private const string PatientFullName = "Juan dela Cruz";

    // ── Sample PatientCode ───────────────────────────────────────────────────
    private const string SamplePatientCode = "PAT-2026-00001";

    /// <summary>
    /// Entry point called from Program.cs on every startup.
    /// All methods are safe to call multiple times.
    /// </summary>
    public static async Task SeedAsync(
        AppDbContext          db,
        RoleManager<IdentityRole>    roleManager,
        UserManager<ApplicationUser> userManager)
    {
        await EnsureRolesAsync(roleManager);

        var adminUser   = await EnsureUserAsync(userManager, AdminEmail,   AdminPassword,   AdminFullName,   AdminRoleName);
        var staffUser   = await EnsureUserAsync(userManager, StaffEmail,   StaffPassword,   StaffFullName,   StaffRoleName);
        var patientUser = await EnsureUserAsync(userManager, PatientEmail, PatientPassword, PatientFullName, PatientRoleName);

        await EnsureSuppliersAsync(db);
        var medicines = await EnsureMedicinesAsync(db);
        var patient   = await EnsurePatientAsync(db, patientUser);
        await EnsurePrescriptionAndDispensingAsync(db, patient, staffUser, medicines);
    }

    // ── Roles ────────────────────────────────────────────────────────────────

    private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = [AdminRoleName, StaffRoleName, PatientRoleName];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create role '{role}': {errors}");
                }
            }
        }
    }

    // ── Users ────────────────────────────────────────────────────────────────

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email, string password, string fullName, string role)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing != null) return existing;

        var user = new ApplicationUser
        {
            UserName       = email,
            Email          = email,
            FullName       = fullName,
            EmailConfirmed = true,
            IsDeleted      = false,
            CreatedAt      = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create user '{email}': {errors}");
        }

        var roleResult = await userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to assign role '{role}' to '{email}': {errors}");
        }

        return user;
    }

    // ── Suppliers ────────────────────────────────────────────────────────────

    private static async Task EnsureSuppliersAsync(AppDbContext db)
    {
        // Use IgnoreQueryFilters() in case soft-delete filter interferes during checks
        if (await db.Suppliers.IgnoreQueryFilters().AnyAsync()) return;

        db.Suppliers.AddRange(
            new Supplier
            {
                Name          = "United Laboratories, Inc. (Unilab)",
                ContactPerson = "Engr. Roberto Reyes",
                ContactNumber = "02-8858-1000",
                Address       = "66 United Street, Mandaluyong City, Metro Manila",
                IsDeleted     = false,
                CreatedAt     = DateTime.UtcNow
            },
            new Supplier
            {
                Name          = "Generika Drugstore (Medilines Distributors)",
                ContactPerson = "Ms. Ana Lim",
                ContactNumber = "02-8887-0000",
                Address       = "Km 29, Aguinaldo Highway, Dasmarinas, Cavite",
                IsDeleted     = false,
                CreatedAt     = DateTime.UtcNow
            }
        );

        await db.SaveChangesAsync();
    }

    // ── Medicines ────────────────────────────────────────────────────────────

    private static async Task<List<Medicine>> EnsureMedicinesAsync(AppDbContext db)
    {
        if (await db.Medicines.IgnoreQueryFilters().AnyAsync())
            return await db.Medicines.IgnoreQueryFilters().ToListAsync();

        var medicines = new List<Medicine>
        {
            new Medicine
            {
                Name          = "Amoxicillin 500mg Capsule",
                Category      = "Antibiotic",
                Stock         = 200,
                MinStockLevel = 30,
                ExpiryDate    = DateTime.Today.AddMonths(18),
                DosageInfo    = "500mg — Take one capsule three times daily for 7 days. Complete the full course.",
                IsDeleted     = false,
                CreatedAt     = DateTime.UtcNow
            },
            new Medicine
            {
                Name          = "Metformin 500mg Tablet",
                Category      = "Antidiabetic",
                Stock         = 150,
                MinStockLevel = 25,
                ExpiryDate    = DateTime.Today.AddMonths(24),
                DosageInfo    = "500mg — Take one tablet twice daily with meals. Monitor blood glucose regularly.",
                IsDeleted     = false,
                CreatedAt     = DateTime.UtcNow
            },
            new Medicine
            {
                Name          = "Amlodipine 5mg Tablet",
                Category      = "Antihypertensive",
                Stock         = 100,
                MinStockLevel = 20,
                ExpiryDate    = DateTime.Today.AddMonths(20),
                DosageInfo    = "5mg — Take one tablet once daily. Do not stop without consulting a physician.",
                IsDeleted     = false,
                CreatedAt     = DateTime.UtcNow
            },
            new Medicine
            {
                Name          = "Atorvastatin 20mg Tablet",
                Category      = "Cholesterol-Lowering (Statin)",
                Stock         = 8,       // intentionally low to trigger LowStock alert
                MinStockLevel = 15,
                ExpiryDate    = DateTime.Today.AddDays(25),  // near expiry to trigger NearExpiry alert
                DosageInfo    = "20mg — Take one tablet at bedtime. Report muscle pain or weakness immediately.",
                IsDeleted     = false,
                CreatedAt     = DateTime.UtcNow
            },
            new Medicine
            {
                Name          = "Paracetamol 500mg Tablet",
                Category      = "Analgesic / Antipyretic",
                Stock         = 500,
                MinStockLevel = 50,
                ExpiryDate    = DateTime.Today.AddMonths(30),
                DosageInfo    = "500mg — Take one to two tablets every 4–6 hours as needed. Do not exceed 8 tablets in 24 hours.",
                IsDeleted     = false,
                CreatedAt     = DateTime.UtcNow
            },
            new Medicine
            {
                Name          = "Mefenamic Acid 500mg Capsule",
                Category      = "Analgesic",
                Stock         = 150,
                MinStockLevel = 25,
                ExpiryDate    = DateTime.Today.AddMonths(30),
                DosageInfo    = "500mg — Take 1 capsule 3x daily after meals as needed for pain.",
                IsDeleted     = false,
                CreatedAt     = DateTime.UtcNow
            },
            new Medicine
            {
                Name          = "Cetirizine 10mg Tablet",
                Category      = "Antihistamine",
                Stock         = 120,
                MinStockLevel = 20,
                ExpiryDate    = DateTime.Today.AddMonths(28),
                DosageInfo    = "10mg — Take 1 tablet once daily at bedtime.",
                IsDeleted     = false,
                CreatedAt     = DateTime.UtcNow
            },
            new Medicine
            {
                Name          = "Losartan 50mg Tablet",
                Category      = "Antihypertensive",
                Stock         = 160,
                MinStockLevel = 25,
                ExpiryDate    = DateTime.Today.AddMonths(24),
                DosageInfo    = "50mg — Take 1 tablet once daily.",
                IsDeleted     = false,
                CreatedAt     = DateTime.UtcNow
            },
            new Medicine
            {
                Name          = "Salbutamol 2mg Tablet",
                Category      = "Bronchodilator",
                Stock         = 90,
                MinStockLevel = 15,
                ExpiryDate    = DateTime.Today.AddMonths(22),
                DosageInfo    = "2mg — Take 1 tablet 3x daily as needed.",
                IsDeleted     = false,
                CreatedAt     = DateTime.UtcNow
            },
            new Medicine
            {
                Name          = "Omeprazole 20mg Capsule",
                Category      = "Gastrointestinal",
                Stock         = 130,
                MinStockLevel = 20,
                ExpiryDate    = DateTime.Today.AddMonths(26),
                DosageInfo    = "20mg — Take 1 capsule once daily before breakfast.",
                IsDeleted     = false,
                CreatedAt     = DateTime.UtcNow
            },
            new Medicine
            {
                Name          = "Cotrimoxazole 400mg/80mg Tablet",
                Category      = "Antibiotic",
                Stock         = 110,
                MinStockLevel = 20,
                ExpiryDate    = DateTime.Today.AddMonths(20),
                DosageInfo    = "400mg/80mg — Take 2 tablets twice daily for 5 days.",
                IsDeleted     = false,
                CreatedAt     = DateTime.UtcNow
            },
            new Medicine
            {
                Name          = "Ferrous Sulfate 325mg Tablet",
                Category      = "Iron Supplement",
                Stock         = 200,
                MinStockLevel = 40,
                ExpiryDate    = DateTime.Today.AddMonths(32),
                DosageInfo    = "325mg — Take 1 tablet once daily.",
                IsDeleted     = false,
                CreatedAt     = DateTime.UtcNow
            },
            new Medicine
            {
                Name          = "Ascorbic Acid 500mg Tablet",
                Category      = "Vitamin / Supplement",
                Stock         = 300,
                MinStockLevel = 50,
                ExpiryDate    = DateTime.Today.AddMonths(36),
                DosageInfo    = "500mg — Take 1 tablet once daily after meals.",
                IsDeleted     = false,
                CreatedAt     = DateTime.UtcNow
            },
            new Medicine
            {
                Name          = "Cloxacillin 500mg Capsule",
                Category      = "Antibiotic",
                Stock         = 80,
                MinStockLevel = 15,
                ExpiryDate    = DateTime.Today.AddMonths(18),
                DosageInfo    = "500mg — Take 1 capsule 4x daily on empty stomach.",
                IsDeleted     = false,
                CreatedAt     = DateTime.UtcNow
            },
            new Medicine
            {
                Name          = "Hydrochlorothiazide 25mg Tablet",
                Category      = "Antihypertensive / Diuretic",
                Stock         = 12,
                MinStockLevel = 20,
                ExpiryDate    = DateTime.Today.AddDays(23),
                DosageInfo    = "25mg — Take 1 tablet once daily in the morning.",
                IsDeleted     = false,
                CreatedAt     = DateTime.UtcNow
            },
            new Medicine
            {
                Name          = "Glibenclamide 5mg Tablet",
                Category      = "Antidiabetic",
                Stock         = 140,
                MinStockLevel = 25,
                ExpiryDate    = DateTime.Today.AddMonths(24),
                DosageInfo    = "5mg — Take 1 tablet once daily before breakfast.",
                IsDeleted     = false,
                CreatedAt     = DateTime.UtcNow
            },
            new Medicine
            {
                Name          = "Isoniazid 300mg Tablet",
                Category      = "Antituberculosis",
                Stock         = 60,
                MinStockLevel = 15,
                ExpiryDate    = DateTime.Today.AddMonths(20),
                DosageInfo    = "300mg — Take 1 tablet once daily on empty stomach.",
                IsDeleted     = false,
                CreatedAt     = DateTime.UtcNow
            },
            new Medicine
            {
                Name          = "Rifampicin 450mg Capsule",
                Category      = "Antituberculosis",
                Stock         = 55,
                MinStockLevel = 15,
                ExpiryDate    = DateTime.Today.AddMonths(19),
                DosageInfo    = "450mg — Take 1 capsule once daily on empty stomach.",
                IsDeleted     = false,
                CreatedAt     = DateTime.UtcNow
            },
            new Medicine
            {
                Name          = "Doxycycline 100mg Capsule",
                Category      = "Antibiotic",
                Stock         = 75,
                MinStockLevel = 15,
                ExpiryDate    = DateTime.Today.AddMonths(21),
                DosageInfo    = "100mg — Take 1 capsule twice daily after meals.",
                IsDeleted     = false,
                CreatedAt     = DateTime.UtcNow
            },
            new Medicine
            {
                Name          = "Ibuprofen 400mg Tablet",
                Category      = "Anti-inflammatory / Analgesic",
                Stock         = 180,
                MinStockLevel = 30,
                ExpiryDate    = DateTime.Today.AddMonths(27),
                DosageInfo    = "400mg — Take 1 tablet 3x daily after meals.",
                IsDeleted     = false,
                CreatedAt     = DateTime.UtcNow
            },
            new Medicine
            {
                Name          = "Prednisone 20mg Tablet",
                Category      = "Corticosteroid",
                Stock         = 70,
                MinStockLevel = 15,
                ExpiryDate    = DateTime.Today.AddMonths(25),
                DosageInfo    = "20mg — As prescribed by physician.",
                IsDeleted     = false,
                CreatedAt     = DateTime.UtcNow
            },
            new Medicine
            {
                Name          = "Folic Acid 400mcg Tablet",
                Category      = "Vitamin / Supplement",
                Stock         = 250,
                MinStockLevel = 40,
                ExpiryDate    = DateTime.Today.AddMonths(34),
                DosageInfo    = "400mcg — Take 1 tablet once daily.",
                IsDeleted     = false,
                CreatedAt     = DateTime.UtcNow
            },
            new Medicine
            {
                Name          = "Vitamin B Complex Tablet",
                Category      = "Vitamin / Supplement",
                Stock         = 220,
                MinStockLevel = 35,
                ExpiryDate    = DateTime.Today.AddMonths(30),
                DosageInfo    = "Take 1 tablet once daily after meals.",
                IsDeleted     = false,
                CreatedAt     = DateTime.UtcNow
            },
            new Medicine
            {
                Name          = "Erythromycin 500mg Tablet",
                Category      = "Antibiotic",
                Stock         = 9,
                MinStockLevel = 20,
                ExpiryDate    = DateTime.Today.AddDays(28),
                DosageInfo    = "500mg — Take 1 tablet 4x daily.",
                IsDeleted     = false,
                CreatedAt     = DateTime.UtcNow
            },
            new Medicine
            {
                Name          = "Zinc Sulfate 20mg Tablet",
                Category      = "Mineral Supplement",
                Stock         = 190,
                MinStockLevel = 30,
                ExpiryDate    = DateTime.Today.AddMonths(33),
                DosageInfo    = "20mg — Take 1 tablet once daily.",
                IsDeleted     = false,
                CreatedAt     = DateTime.UtcNow
            }
        };

        db.Medicines.AddRange(medicines);
        await db.SaveChangesAsync();

        // Seed stock alerts for low-stock and near-expiry medicines
        var atorvastatin = medicines.First(m => m.Name.Contains("Atorvastatin"));
        var hydrochlorothiazide = medicines.First(m => m.Name.Contains("Hydrochlorothiazide"));
        var erythromycin = medicines.First(m => m.Name.Contains("Erythromycin"));

        db.StockAlerts.AddRange(
            new StockAlert
            {
                MedicineId = atorvastatin.Id,
                AlertType  = AlertType.LowStock,
                Message    = $"Atorvastatin 20mg Tablet stock ({atorvastatin.Stock} units) is below minimum level ({atorvastatin.MinStockLevel} units).",
                IsResolved = false,
                CreatedAt  = DateTime.UtcNow
            },
            new StockAlert
            {
                MedicineId = atorvastatin.Id,
                AlertType  = AlertType.NearExpiry,
                Message    = $"Atorvastatin 20mg Tablet expires on {atorvastatin.ExpiryDate:MMMM dd, yyyy} — within 30 days.",
                IsResolved = false,
                CreatedAt  = DateTime.UtcNow
            },
            new StockAlert
            {
                MedicineId = hydrochlorothiazide.Id,
                AlertType  = AlertType.LowStock,
                Message    = $"Hydrochlorothiazide 25mg Tablet stock ({hydrochlorothiazide.Stock} units) is below minimum level ({hydrochlorothiazide.MinStockLevel} units).",
                IsResolved = false,
                CreatedAt  = DateTime.UtcNow
            },
            new StockAlert
            {
                MedicineId = hydrochlorothiazide.Id,
                AlertType  = AlertType.NearExpiry,
                Message    = $"Hydrochlorothiazide 25mg Tablet expires on {hydrochlorothiazide.ExpiryDate:MMMM dd, yyyy} — within 30 days.",
                IsResolved = false,
                CreatedAt  = DateTime.UtcNow
            },
            new StockAlert
            {
                MedicineId = erythromycin.Id,
                AlertType  = AlertType.LowStock,
                Message    = $"Erythromycin 500mg Tablet stock ({erythromycin.Stock} units) is below minimum level ({erythromycin.MinStockLevel} units).",
                IsResolved = false,
                CreatedAt  = DateTime.UtcNow
            },
            new StockAlert
            {
                MedicineId = erythromycin.Id,
                AlertType  = AlertType.NearExpiry,
                Message    = $"Erythromycin 500mg Tablet expires on {erythromycin.ExpiryDate:MMMM dd, yyyy} — within 30 days.",
                IsResolved = false,
                CreatedAt  = DateTime.UtcNow
            }
        );
        await db.SaveChangesAsync();

        return medicines;
    }

    // ── Patient profile ──────────────────────────────────────────────────────

    private static async Task<Patient> EnsurePatientAsync(
        AppDbContext db, ApplicationUser patientUser)
    {
        var existing = await db.Patients
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.LinkedUserId == patientUser.Id);

        if (existing != null) return existing;

        var patient = new Patient
        {
            PatientCode   = SamplePatientCode,
            FullName      = patientUser.FullName,
            Birthdate     = new DateTime(1990, 6, 15),
            Address       = "Blk 5 Lot 2, Barangay Halang, Calamba City, Laguna",
            ContactNumber = "09171234567",
            LinkedUserId  = patientUser.Id,
            IsDeleted     = false,
            CreatedAt     = DateTime.UtcNow
        };

        db.Patients.Add(patient);
        await db.SaveChangesAsync();
        return patient;
    }

    // ── Prescription + DispensingRecord ──────────────────────────────────────

    private static async Task EnsurePrescriptionAndDispensingAsync(
        AppDbContext db,
        Patient patient,
        ApplicationUser staffUser,
        List<Medicine> medicines)
    {
        if (await db.Prescriptions.IgnoreQueryFilters().AnyAsync()) return;

        var amoxicillin = medicines.First(m => m.Name.Contains("Amoxicillin"));

        var prescription = new Prescription
        {
            PatientId  = patient.Id,
            StaffId    = staffUser.Id,
            MedicineId = amoxicillin.Id,
            DoctorName = "Dr. Lourdes Aquino",
            Dosage     = "500mg — one capsule three times daily",
            Duration   = "7 days",
            Status     = PrescriptionStatus.Active,
            IsDeleted  = false,
            CreatedAt  = DateTime.UtcNow
        };

        db.Prescriptions.Add(prescription);
        await db.SaveChangesAsync();

        // Dispensing record — deducts stock from Amoxicillin
        const int dispensedQty = 21; // 3 times/day × 7 days

        var dispensingRecord = new DispensingRecord
        {
            PrescriptionId    = prescription.Id,
            PatientId         = patient.Id,
            StaffId           = staffUser.Id,
            QuantityDispensed = dispensedQty,
            DateDispensed     = DateTime.UtcNow,
            Notes             = "Initial dispensing for 7-day antibiotic course."
        };

        db.DispensingRecords.Add(dispensingRecord);

        // Deduct stock
        amoxicillin.Stock -= dispensedQty;

        await db.SaveChangesAsync();

        // Audit log for the seed dispensing
        db.AuditLogs.Add(new AuditLog
        {
            UserId        = staffUser.Id,
            Action        = "Dispensed",
            TableAffected = "DispensingRecords",
            RecordId      = dispensingRecord.Id.ToString(),
            Details       = $"Dispensed {dispensedQty} units of Amoxicillin 500mg to patient {patient.PatientCode}.",
            Timestamp     = DateTime.UtcNow,
            IPAddress     = "127.0.0.1"
        });

        await db.SaveChangesAsync();
    }
}
