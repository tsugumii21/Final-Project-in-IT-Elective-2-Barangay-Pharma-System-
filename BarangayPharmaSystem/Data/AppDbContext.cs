using BarangayPharmaSystem.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BarangayPharmaSystem.Data;

/// <summary>
/// Main EF Core database context for the Barangay Pharma System.
/// Extends IdentityDbContext to incorporate ASP.NET Core Identity tables.
/// Global query filters are applied to all soft-deletable entities.
/// </summary>
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ── Application DbSets ───────────────────────────────────────────────────

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Medicine> Medicines => Set<Medicine>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<DispensingRecord> DispensingRecords => Set<DispensingRecord>();
    public DbSet<RefillRequest> RefillRequests => Set<RefillRequest>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<StockAlert> StockAlerts => Set<StockAlert>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── Rename Identity tables to cleaner names ──────────────────────────
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<IdentityRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

        // ── Global query filters (soft delete) ───────────────────────────────
        // Excluded from queries automatically; use .IgnoreQueryFilters() to bypass.
        builder.Entity<ApplicationUser>().HasQueryFilter(u => !u.IsDeleted);
        builder.Entity<Patient>().HasQueryFilter(p => !p.IsDeleted);
        builder.Entity<Medicine>().HasQueryFilter(m => !m.IsDeleted);
        builder.Entity<Supplier>().HasQueryFilter(s => !s.IsDeleted);
        builder.Entity<Prescription>().HasQueryFilter(p => !p.IsDeleted);

        // ── Patient ──────────────────────────────────────────────────────────
        builder.Entity<Patient>(entity =>
        {
            entity.HasIndex(p => p.PatientCode).IsUnique();

            // One patient → one optional user account
            entity.HasOne(p => p.LinkedUser)
                  .WithOne(u => u.LinkedPatient)
                  .HasForeignKey<Patient>(p => p.LinkedUserId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ── Prescription ─────────────────────────────────────────────────────
        builder.Entity<Prescription>(entity =>
        {
            entity.HasOne(p => p.Patient)
                  .WithMany(pat => pat.Prescriptions)
                  .HasForeignKey(p => p.PatientId)
                  .OnDelete(DeleteBehavior.Restrict);

            // WORKAROUND: Use NoAction to avoid multiple cascade paths in SQL Server
            entity.HasOne(p => p.Staff)
                  .WithMany(u => u.WrittenPrescriptions)
                  .HasForeignKey(p => p.StaffId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(p => p.Medicine)
                  .WithMany(m => m.Prescriptions)
                  .HasForeignKey(p => p.MedicineId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── DispensingRecord ─────────────────────────────────────────────────
        builder.Entity<DispensingRecord>(entity =>
        {
            entity.HasOne(d => d.Prescription)
                  .WithMany(p => p.DispensingRecords)
                  .HasForeignKey(d => d.PrescriptionId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Patient)
                  .WithMany(p => p.DispensingRecords)
                  .HasForeignKey(d => d.PatientId)
                  .OnDelete(DeleteBehavior.Restrict);

            // WORKAROUND: NoAction to avoid multiple cascade paths
            entity.HasOne(d => d.Staff)
                  .WithMany(u => u.DispensingRecords)
                  .HasForeignKey(d => d.StaffId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        // ── RefillRequest ────────────────────────────────────────────────────
        builder.Entity<RefillRequest>(entity =>
        {
            entity.HasOne(r => r.Prescription)
                  .WithMany(p => p.RefillRequests)
                  .HasForeignKey(r => r.PrescriptionId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Patient)
                  .WithMany(p => p.RefillRequests)
                  .HasForeignKey(r => r.PatientId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── AuditLog ─────────────────────────────────────────────────────────
        builder.Entity<AuditLog>(entity =>
        {
            // Nullable FK — logs survive if the user is deleted
            entity.HasOne(a => a.User)
                  .WithMany(u => u.AuditLogs)
                  .HasForeignKey(a => a.UserId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ── StockAlert ───────────────────────────────────────────────────────
        builder.Entity<StockAlert>(entity =>
        {
            entity.HasOne(s => s.Medicine)
                  .WithMany(m => m.StockAlerts)
                  .HasForeignKey(s => s.MedicineId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
