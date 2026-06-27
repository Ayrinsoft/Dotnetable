using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.API.Controllers;

/// <summary>
/// Serves files held by the <see cref="StorageProviderType.LocalHost"/> backend straight from the API
/// host's disk. Both the admin dashboard and the public site reach local files through this endpoint.
/// </summary>
[ApiController]
[Route("api/files")]
public sealed class FilesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public FilesController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    /// <summary>Streams a locally-stored file (or its thumbnail) by website and stored name.</summary>
    [HttpGet("{websiteId:int}/{*storedName}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(int websiteId, string storedName, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(storedName)) return NotFound();

        var fileName = Path.GetFileName(storedName);
        var relPath = $"{websiteId}/{fileName}";

        var record = await _db.FileRecords.AsNoTracking()
            .Include(f => f.WebsiteStorageSettings)
            .FirstOrDefaultAsync(f =>
                f.WebsiteStorageSettings.WebsiteID == websiteId
                && f.StorageProvider == (short)StorageProviderType.LocalHost
                && !f.IsDeleted
                && (f.StoragePath == relPath || f.ThumbnailStorage == relPath), ct);

        if (record is null) return NotFound();

        var isThumbnail = record.ThumbnailStorage == relPath;
        var settings = LocalStorageProvider.Parse(record.WebsiteStorageSettings.StorageSettingsJSON);
        var full = Path.Combine(LocalStorageProvider.ResolveRoot(_env.ContentRootPath, settings), relPath);

        if (!System.IO.File.Exists(full)) return NotFound();

        var mime = isThumbnail ? "image/jpeg" : (record.MimeType ?? "application/octet-stream");
        return PhysicalFile(full, mime);
    }
}
