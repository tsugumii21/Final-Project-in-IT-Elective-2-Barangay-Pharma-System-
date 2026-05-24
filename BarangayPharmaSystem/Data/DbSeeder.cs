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
            }
        };

        db.Medicines.AddRange(medicines);
        await db.SaveChangesAsync();

        // Seed stock alerts for the low-stock and near-expiry medicine (Atorvastatin)
        var atorvastatin = medicines.First(m => m.Name.Contains("Atorvastatin"));
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
