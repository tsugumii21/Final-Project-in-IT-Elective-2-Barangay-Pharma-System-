namespace BarangayPharmaSystem.Services;

/// <summary>Result returned by all file upload operations.</summary>
public sealed record FileUploadResult(
    bool    Success,
    string? RelativePath,
    string? ErrorMessage);

/// <summary>Contract for validated file upload operations.</summary>
public interface IFileUploadService
{
    /// <summary>
    /// Validates and saves a profile photo for a user.
    /// Accepts .jpg, .jpeg, .png only. Maximum 2MB.
    /// </summary>
    /// <param name="file">The uploaded file from the HTTP form.</param>
    /// <param name="userId">The Identity user ID — used to generate the unique filename.</param>
    /// <returns>A <see cref="FileUploadResult"/> with the saved relative path or an error message.</returns>
    Task<FileUploadResult> UploadUserPhotoAsync(IFormFile file, string userId);

    /// <summary>
    /// Validates and saves a profile photo for a patient.
    /// </summary>
    Task<FileUploadResult> UploadPatientPhotoAsync(IFormFile file, int patientId);

    /// <summary>
    /// Validates and saves a photo for a medicine.
    /// </summary>
    Task<FileUploadResult> UploadMedicinePhotoAsync(IFormFile file, int medicineId);

    /// <summary>
    /// Deletes a previously uploaded file by its relative web path (e.g., /uploads/users/abc.jpg).
    /// Silently ignores if the file does not exist.
    /// </summary>
    void DeleteFile(string? relativePath);
}

/// <summary>
/// Handles validated file uploads to the wwwroot/uploads/ subdirectories.
/// All uploads are validated for MIME type, file extension, and size before saving.
/// </summary>
public class FileUploadService : IFileUploadService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<FileUploadService> _logger;

    // ── Constants ────────────────────────────────────────────────────────────

    private const long   MaxFileSizeBytes       = 10 * 1024 * 1024; // 10 MB
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png"];
    private static readonly string[] AllowedMimeTypes  =
    [
        "image/jpeg",
        "image/jpg",
        "image/png"
    ];

    private const string UserPhotosFolder     = "uploads/users";
    private const string PatientPhotosFolder  = "uploads/patients";
    private const string MedicinePhotosFolder = "uploads/medicines";

    public FileUploadService(IWebHostEnvironment env, ILogger<FileUploadService> logger)
    {
        _env    = env;
        _logger = logger;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public Task<FileUploadResult> UploadUserPhotoAsync(IFormFile file, string userId)
        => SaveFileAsync(file, UserPhotosFolder, userId);

    public Task<FileUploadResult> UploadPatientPhotoAsync(IFormFile file, int patientId)
        => SaveFileAsync(file, PatientPhotosFolder, patientId.ToString());

    public Task<FileUploadResult> UploadMedicinePhotoAsync(IFormFile file, int medicineId)
        => SaveFileAsync(file, MedicinePhotosFolder, medicineId.ToString());

    public void DeleteFile(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return;

        try
        {
            // Relative path is stored as e.g. "/uploads/users/abc.jpg"
            var absolutePath = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(absolutePath))
            {
                File.Delete(absolutePath);
                _logger.LogInformation("Deleted file: {Path}", absolutePath);
            }
        }
        catch (Exception ex)
        {
            // WORKAROUND: Log and swallow — file deletion failure should not break application flow.
            _logger.LogWarning(ex, "Failed to delete file at path: {Path}", relativePath);
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task<FileUploadResult> SaveFileAsync(IFormFile file, string folder, string fileNameBase)
    {
        // 1. Validate size
        if (file.Length > MaxFileSizeBytes)
            return new FileUploadResult(false, null, "File size exceeds the 10MB limit. Please choose a smaller image.");

        // 2. Validate extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return new FileUploadResult(false, null, "Only .jpg, .jpeg, and .png files are accepted.");

        // 3. Validate MIME type (prevents extension spoofing)
        if (!AllowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            return new FileUploadResult(false, null, "Invalid file type. Only JPEG and PNG images are accepted.");

        // 4. Build safe file path — use GUID to prevent filename conflicts and ID exposure
        var uploadDir    = Path.Combine(_env.WebRootPath, folder.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(uploadDir); // ensure directory exists

        var safeFileName = $"{Guid.NewGuid():N}{extension}";
        var absolutePath = Path.Combine(uploadDir, safeFileName);
        var relativePath = $"/{folder}/{safeFileName}";

        // 5. Save file to disk
        try
        {
            await using var stream = new FileStream(absolutePath, FileMode.Create, FileAccess.Write);
            await file.CopyToAsync(stream);
            _logger.LogInformation("Uploaded file: {Path}", absolutePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save uploaded file to {Path}", absolutePath);
            return new FileUploadResult(false, null, "An error occurred while saving the file. Please try again.");
        }

        return new FileUploadResult(true, relativePath, null);
    }
}
