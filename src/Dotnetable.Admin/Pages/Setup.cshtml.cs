using System.ComponentModel.DataAnnotations;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dotnetable.Admin.Pages;

public class SetupModel : PageModel
{
    private readonly ISetupService _setupService;

    public SetupModel(ISetupService setupService)
    {
        _setupService = setupService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }
    public string? TestMessage { get; set; }
    public bool TestSucceeded { get; set; }

    public class InputModel
    {
        // Database connection
        [Required, StringLength(128)] public string DbServer { get; set; } = "localhost";
        [Range(1, 65535)] public int DbPort { get; set; } = 3306;
        [Required, StringLength(64)] public string DbName { get; set; } = "Dotnetable";
        [Required, StringLength(64)] public string DbUser { get; set; } = "root";
        [StringLength(128)] public string DbPassword { get; set; } = string.Empty;
        public bool CreateDatabaseIfMissing { get; set; } = true;

        // Website
        [Required, StringLength(32)] public string TradeName { get; set; } = string.Empty;
        [Required, StringLength(60)] public string BrandName { get; set; } = string.Empty;
        [Required, StringLength(60)] public string WebsiteAddress { get; set; } = string.Empty;
        [Required, StringLength(30)] public string Manager { get; set; } = string.Empty;
        [Required, StringLength(15)] public string Mobile { get; set; } = string.Empty;
        [Required, EmailAddress, StringLength(60)] public string WebsiteEmail { get; set; } = string.Empty;
        [Required, StringLength(2, MinimumLength = 2)] public string DefaultLanguageCode { get; set; } = "en";

        // Administrator
        [Required, StringLength(64)] public string Username { get; set; } = string.Empty;
        [Required, StringLength(256, MinimumLength = 6), DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        [Required, EmailAddress, StringLength(64)] public string Email { get; set; } = string.Empty;
        [Required, StringLength(64)] public string Givenname { get; set; } = string.Empty;
        [Required, StringLength(64)] public string Surname { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        if (await _setupService.IsSetupCompletedAsync(ct))
            return RedirectToPage("/Login");
        return Page();
    }

    /// <summary>Validates the database connection without creating anything.</summary>
    public async Task<IActionResult> OnPostTestAsync(CancellationToken ct)
    {
        if (await _setupService.IsSetupCompletedAsync(ct))
            return RedirectToPage("/Login");

        var result = await _setupService.TestConnectionAsync(BuildDatabaseInfo(), ct);
        TestSucceeded = result.Success;
        TestMessage = result.Message;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (await _setupService.IsSetupCompletedAsync(ct))
            return RedirectToPage("/Login");

        if (!ModelState.IsValid) return Page();

        var request = new SetupRequest
        {
            Database = BuildDatabaseInfo(),
            TradeName = Input.TradeName,
            BrandName = Input.BrandName,
            WebsiteAddress = Input.WebsiteAddress,
            Manager = Input.Manager,
            Mobile = Input.Mobile,
            WebsiteEmail = Input.WebsiteEmail,
            DefaultLanguageCode = Input.DefaultLanguageCode,
            Username = Input.Username,
            Password = Input.Password,
            Email = Input.Email,
            Givenname = Input.Givenname,
            Surname = Input.Surname,
        };

        try
        {
            await _setupService.CompleteSetupAsync(request, ct);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Setup failed: {ex.Message}";
            return Page();
        }

        return RedirectToPage("/Login");
    }

    private DatabaseConnectionInfo BuildDatabaseInfo() => new()
    {
        Server = Input.DbServer,
        Port = Input.DbPort,
        DatabaseName = Input.DbName,
        Username = Input.DbUser,
        Password = Input.DbPassword,
        CreateDatabaseIfMissing = Input.CreateDatabaseIfMissing,
    };
}
