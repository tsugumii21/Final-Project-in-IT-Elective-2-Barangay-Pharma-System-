using BarangayPharmaSystem.Data;
using BarangayPharmaSystem.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace BarangayPharmaSystem.Services;

/// <summary>Contract for recording audit log entries throughout the application.</summary>
public interface IAuditService
{
    /// <summary>
    /// Records an audit event to the AuditLogs table.
    /// Automatically captures the current user's ID and request IP address.
    /// </summary>
    /// <param name="action">Human-readable action (e.g., "Login", "Created", "Dispensed").</param>
    /// <param name="tableAffected">Name of the affected entity/table (e.g., "Users", "Medicines").</param>
    /// <param name="recordId">Primary key of the record affected. Pass null for session events.</param>
    /// <param name="details">Optional extra context (changed values, reason, etc.).</param>
    Task LogAsync(
        string  action,
        string  tableAffected,
        string? recordId = null,
        string? details  = null);
}

/// <summary>
/// Centralised audit logging service.
/// Captures who did what, to which record, from which IP, and when.
/// Injected via DI as a scoped service.
/// </summary>
public class AuditService : IAuditService
{
    private readonly AppDbContext                _db;
    private readonly IHttpContextAccessor        _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuditService(
        AppDbContext                db,
        IHttpContextAccessor        httpContextAccessor,
        UserManager<ApplicationUser> userManager)
    {
        _db                  = db;
        _httpContextAccessor = httpContextAccessor;
        _userManager         = userManager;
    }

    /// <inheritdoc />
    public async Task LogAsync(
        string  action,
        string  tableAffected,
        string? recordId = null,
        string? details  = null)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        // Resolve current user ID from Identity claims (null for anonymous/system events)
        string? userId = null;
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(httpContext.User);
            userId = user?.Id;
        }

        // Resolve client IP address (supports reverse proxy / X-Forwarded-For)
        var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
        if (httpContext?.Request?.Headers.TryGetValue("X-Forwarded-For", out var forwardedIp) == true
            && !string.IsNullOrWhiteSpace(forwardedIp))
        {
            ipAddress = forwardedIp.ToString().Split(',')[0].Trim();
        }

        var log = new AuditLog
        {
            UserId        = userId,
            Action        = action,
            TableAffected = tableAffected,
            RecordId      = recordId,
            Details       = details,
            Timestamp     = DateTime.UtcNow,
            IPAddress     = ipAddress
        };

        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync();
    }
}
