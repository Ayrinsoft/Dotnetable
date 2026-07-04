using Dotnetable.Application.Authorization;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>Customer-facing payment actions: pay an order with wallet balance, or submit an
/// already-uploaded bank-transfer receipt for manual admin verification.</summary>
[Authorize(Policy = RoleKeys.ClientPurchase)]
public class PaymentsController : BaseController
{
    private readonly IPaymentService _paymentService;
    private readonly IWebsiteService _websiteService;
    private readonly IBankAccountService _bankAccounts;
    private readonly IFileService _files;
    private readonly IStorageSettingService _storageSettings;

    public PaymentsController(
        IPaymentService paymentService, IWebsiteService websiteService, IBankAccountService bankAccounts,
        IFileService files, IStorageSettingService storageSettings)
    {
        _paymentService = paymentService;
        _websiteService = websiteService;
        _bankAccounts = bankAccounts;
        _files = files;
        _storageSettings = storageSettings;
    }

    /// <summary>Uploads a bank-transfer receipt image into the media library and returns its file id,
    /// for use with <see cref="SubmitReceipt"/>. Uses the website's first active storage backend.</summary>
    [HttpPost("receipt-upload")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadReceipt(IFormFile file, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });
        if (file.Length == 0) return BadRequest(new { message = "No file uploaded." });

        var storages = await _storageSettings.GetActiveForWebsiteAsync(website.WebsiteID, ct);
        var storage = storages.FirstOrDefault();
        if (storage is null) return StatusCode(503, new { message = "No storage backend is configured for this website." });

        await using var stream = file.OpenReadStream();
        var record = await _files.UploadAsync(new FileUploadRequest
        {
            WebsiteID = website.WebsiteID,
            StorageSettingID = storage.WebsiteStorageSettingsID,
            Content = stream,
            OriginalFileName = file.FileName,
            MimeType = file.ContentType,
            Title = "Payment receipt",
        }, ct);

        return Ok(new { fileId = record.FileRecordID });
    }

    /// <summary>The website's own bank accounts customers may transfer to for manual/offline payment.</summary>
    [HttpGet("bank-accounts")]
    public async Task<IActionResult> GetOfflineBankAccounts(CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var accounts = await _bankAccounts.GetOfflinePaymentAccountsAsync(website.WebsiteID, ct);
        return Ok(accounts.Select(a => new { a.BankAccountID, BankName = a.Bank.Name, a.Title, a.OwnerName, a.IBAN, a.CardNumber }));
    }

    public sealed record WalletPaymentRequest(int OrderId);

    [HttpPost("wallet")]
    public async Task<IActionResult> PayWithWallet([FromBody] WalletPaymentRequest request, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var (success, error, payment) = await _paymentService.PayWithWalletAsync(website.WebsiteID, CurrentClientId, request.OrderId, ct);
        return success ? Ok(new { paymentId = payment!.PaymentID }) : BadRequest(new { message = error });
    }

    public sealed record ReceiptPaymentRequest(int OrderId, int BankAccountId, int ReceiptFileId);

    /// <summary>The receipt image must already be uploaded via the media library (returns a FileRecord id) before calling this.</summary>
    [HttpPost("receipt")]
    public async Task<IActionResult> SubmitReceipt([FromBody] ReceiptPaymentRequest request, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var (success, error, payment) = await _paymentService.SubmitBankReceiptAsync(
            website.WebsiteID, CurrentClientId, request.OrderId, request.BankAccountId, request.ReceiptFileId, ct);
        return success ? Ok(new { paymentId = payment!.PaymentID }) : BadRequest(new { message = error });
    }

    [HttpGet("{orderId:int}/status")]
    public async Task<IActionResult> GetStatus(int orderId, CancellationToken ct = default)
    {
        var payment = await _paymentService.GetLatestForOrderAsync(orderId, ct);
        return payment is null ? NotFound() : Ok(new { payment.PaymentID, payment.Status, payment.Method, payment.PaidAt });
    }

    private int CurrentClientId =>
        int.TryParse(User.FindFirst(ClientClaims.ClientId)?.Value, out var id)
            ? id
            : throw new InvalidOperationException("Token does not carry a client id.");
}
