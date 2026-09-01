using Dotnetable.Domain.Entities;
using Dotnetable.Application.Authorization;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Dotnetable.Hosting;

namespace Dotnetable.API.Controllers;

/// <summary>Customer-facing payment actions: pay an order with wallet balance, or submit an
/// already-uploaded bank-transfer receipt for manual admin verification.</summary>
[Authorize(Policy = RoleKeys.ClientPurchase)]
[EnableRateLimiting(RateLimiting.CheckoutPolicy)]
public class PaymentsController : BaseController
{
    private readonly IPaymentService _paymentService;
    private readonly IWebsiteService _websiteService;
    private readonly IBankAccountService _bankAccounts;
    private readonly IFileService _files;
    private readonly IStorageSettingService _storageSettings;
    private readonly IOnlinePaymentService _online;

    public PaymentsController(
        IPaymentService paymentService, IWebsiteService websiteService, IBankAccountService bankAccounts,
        IFileService files, IStorageSettingService storageSettings, IOnlinePaymentService online)
    {
        _paymentService = paymentService;
        _websiteService = websiteService;
        _bankAccounts = bankAccounts;
        _files = files;
        _storageSettings = storageSettings;
        _online = online;
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

    /// <summary>Online gateways this website can take payment through, for the checkout picker.</summary>
    [HttpGet("gateways")]
    public async Task<IActionResult> GetGateways(CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var gateways = await _online.GetAvailableAsync(website.WebsiteID, ct);
        return Ok(gateways.Select(g => new
        {
            g.PaymentGatewayID,
            g.Name,
            g.Provider,
            g.ProviderDisplayName,
            g.IsSandbox,
        }));
    }

    public sealed record StartOnlinePaymentRequest(int OrderId, int GatewayId, string ReturnUrl);

    /// <summary>
    /// Starts an online payment and returns the gateway URL to send the payer to. The order is not
    /// touched here — only the verified callback can mark it paid.
    /// </summary>
    [HttpPost("online/start")]
    public async Task<IActionResult> StartOnline([FromBody] StartOnlinePaymentRequest request, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        // The return URL comes from the storefront, and an open redirect here would let a phishing
        // page borrow the shop's checkout flow. Only the site's own address is accepted.
        if (!IsOwnSiteUrl(website, request.ReturnUrl))
            return BadRequest(new { message = "The return URL must belong to this website." });

        var result = await _online.StartAsync(
            website.WebsiteID, CurrentClientId, request.OrderId, request.GatewayId, request.ReturnUrl, ct);

        return result.Success
            ? Ok(new { redirectUrl = result.RedirectUrl })
            : BadRequest(new { message = result.Error });
    }

    /// <summary>
    /// Completes a payment from the gateway's return. Anonymous because the payer arrives via the
    /// PSP and may not carry their bearer token; safety comes from the server-to-server verify, not
    /// from who calls this.
    /// </summary>
    [HttpPost("online/callback")]
    [HttpGet("online/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> OnlineCallback(CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        // PSPs differ on GET vs POST and on field names, so both are collected and the provider picks.
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in Request.Query)
            values[key] = value.ToString();

        if (Request.HasFormContentType)
        {
            foreach (var (key, value) in Request.Form)
                values[key] = value.ToString();
        }

        var callback = new GatewayCallback(
            Authority: Find(values, "Authority", "authority", "trackId", "token", "trans_id", "id", "session_id", "refId", "refid"),
            Status: Find(values, "Status", "status", "result"),
            Values: values);

        var result = await _online.CompleteAsync(website.WebsiteID, callback, ct);

        return result.Paid
            ? Ok(new { paid = true, referenceNumber = result.ReferenceNumber })
            : BadRequest(new { paid = false, message = result.Error });
    }

    private static string? Find(IReadOnlyDictionary<string, string> values, params string[] names)
    {
        foreach (var name in names)
        {
            if (values.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value))
                return value;
        }
        return null;
    }

    /// <summary>True when <paramref name="url"/> is an absolute URL on this website's own host.</summary>
    private static bool IsOwnSiteUrl(Website website, string? url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return false;
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) return false;

        var configured = website.WebsiteAddress?.Trim();
        if (string.IsNullOrWhiteSpace(configured)) return false;

        // WebsiteAddress is stored as a bare host in some installs and a full URL in others.
        var host = Uri.TryCreate(configured, UriKind.Absolute, out var configuredUri)
            ? configuredUri.Host
            : configured;

        return string.Equals(uri.Host, host, StringComparison.OrdinalIgnoreCase);
    }

    [HttpGet("{orderId:int}/status")]
    public async Task<IActionResult> GetStatus(int orderId, CancellationToken ct = default)
    {
        var payment = await _paymentService.GetLatestForOrderAsync(orderId, CurrentClientId, ct);
        return payment is null ? NotFound() : Ok(new { payment.PaymentID, payment.Status, payment.Method, payment.PaidAt });
    }

    private int CurrentClientId =>
        int.TryParse(User.FindFirst(ClientClaims.ClientId)?.Value, out var id)
            ? id
            : throw new InvalidOperationException("Token does not carry a client id.");
}
