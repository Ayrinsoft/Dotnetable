using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;

namespace Dotnetable.Hosting;

/// <summary>
/// Per-IP request limits.
///
/// <para>The account-level limits (OTP attempt budget, sign-in lockout, resend cooldown) stop one
/// attacker grinding one account. They do nothing about the other shape of the attack: a script
/// spraying one common password across thousands of accounts, or hammering an unauthenticated
/// endpoint until the database gives up. That is what these limits are for, and why both layers
/// exist rather than either alone.</para>
///
/// <para>Anything that costs money or grants credentials gets the tight bucket; ordinary catalogue
/// reads get a loose global bucket that only trips under abuse.</para>
/// </summary>
public static class RateLimiting
{
    /// <summary>Sign-in, registration, refresh — anything that hands out or checks credentials.</summary>
    public const string AuthPolicy = "dn-auth";

    /// <summary>Endpoints that send an email or an SMS, i.e. that spend real money per request.</summary>
    public const string OtpPolicy = "dn-otp";

    /// <summary>Public writes: reviews, questions, contact forms, form submissions.</summary>
    public const string PublicWritePolicy = "dn-public-write";

    /// <summary>Checkout and payment actions.</summary>
    public const string CheckoutPolicy = "dn-checkout";

    /// <summary>File/receipt/photo uploads.</summary>
    public const string UploadPolicy = "dn-upload";

    /// <summary>
    /// Registers the limiter with a loose global fallback plus the named policies above.
    /// <paramref name="trustForwardedFor"/> should only be true when the app genuinely sits behind a
    /// reverse proxy that overwrites the header — otherwise a client can forge it and get a fresh
    /// bucket per request, which is worse than having no limiter at all.
    /// </summary>
    public static IServiceCollection AddDotnetableRateLimiting(
        this IServiceCollection services,
        bool trustForwardedFor = false)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, ct) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                }

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    error = "too_many_requests",
                    message = "Too many requests. Please wait a moment and try again.",
                }, ct);
            };

            // Global backstop. Generous enough that a real visitor browsing a catalogue never sees it.
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    ClientKey(context, trustForwardedFor),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 300,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                    }));

            // Credential endpoints. Ten a minute is far more than a person needs and far less than a
            // password sprayer wants.
            AddFixedWindow(options, AuthPolicy, permits: 10, window: TimeSpan.FromMinutes(1), trustForwardedFor);

            // Delivery endpoints. Each request may bill the site for an SMS, so the window is long.
            AddFixedWindow(options, OtpPolicy, permits: 5, window: TimeSpan.FromMinutes(15), trustForwardedFor);

            AddFixedWindow(options, PublicWritePolicy, permits: 20, window: TimeSpan.FromMinutes(10), trustForwardedFor);
            AddFixedWindow(options, CheckoutPolicy, permits: 30, window: TimeSpan.FromMinutes(5), trustForwardedFor);
            AddFixedWindow(options, UploadPolicy, permits: 30, window: TimeSpan.FromMinutes(10), trustForwardedFor);
        });

        return services;
    }

    private static void AddFixedWindow(
        RateLimiterOptions options, string policyName, int permits, TimeSpan window, bool trustForwardedFor)
    {
        options.AddPolicy(policyName, context =>
            RateLimitPartition.GetFixedWindowLimiter(
                $"{policyName}:{ClientKey(context, trustForwardedFor)}",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = permits,
                    Window = window,
                    QueueLimit = 0,
                }));
    }

    /// <summary>
    /// The partition key: an authenticated caller is limited as themselves (so one office NAT does
    /// not share a bucket), everyone else by remote IP.
    /// </summary>
    private static string ClientKey(HttpContext context, bool trustForwardedFor)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var name = context.User.Identity.Name;
            if (!string.IsNullOrWhiteSpace(name)) return "u:" + name;
        }

        if (trustForwardedFor &&
            context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
        {
            var first = forwarded.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(first)) return "ip:" + first.Trim();
        }

        return "ip:" + (context.Connection.RemoteIpAddress?.ToString() ?? "unknown");
    }
}
