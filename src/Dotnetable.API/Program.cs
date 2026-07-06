using System.Data.Common;
using System.Security.Claims;
using System.Text;
using Asp.Versioning;
using Dotnetable.API.Auth;
using Dotnetable.Application.Authorization;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// ── JWT bearer authentication for website clients (and any token-based caller) ──
var jwt = new JwtSettings();
builder.Configuration.GetSection(JwtSettings.SectionName).Bind(jwt);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(string.IsNullOrWhiteSpace(jwt.SigningKey)
                    ? new string('0', 32) // placeholder so startup never crashes when JWT is unconfigured
                    : jwt.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            RoleClaimType = ClaimTypes.Role,
        };
    });

builder.Services.AddAuthorization(ApiAuthorization.Register);

// API versioning is driven by the X-Api-Version request header (defaults to 1.0 when absent).
builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true; // echoes api-supported-versions in the response headers
        options.ApiVersionReader = new HeaderApiVersionReader("X-Api-Version");
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = false;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Dotnetable API", Version = "v1" });
});

// CORS for browser-based front-ends (the React SPA in serverless mode calls the API directly).
// Allowed origins come from configuration; when none are configured any origin is accepted, since
// every request is already scoped/authenticated by the X-Website-Key header + client JWTs.
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
{
    if (corsOrigins.Length > 0)
        policy.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod();
    else
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
}));

builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.ContentRootPath);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Surfaces DB connectivity/config failures (bad connection string, unreachable server, cert
// mismatch, etc.) as a clear 503 instead of a cryptic RetryLimitExceededException/SqlException
// stack trace — this is exactly what a misconfigured localsettings.json produces.
app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var error = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    if (error is DbException or RetryLimitExceededException)
    {
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        await context.Response.WriteAsJsonAsync(new
        {
            error = "database_unavailable",
            message = "Could not connect to the database. Check the Database section in this app's localsettings.json (provider, connection string, credentials).",
            detail = app.Environment.IsDevelopment() ? error.Message : null,
        });
        return;
    }

    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await context.Response.WriteAsJsonAsync(new
    {
        error = "server_error",
        detail = app.Environment.IsDevelopment() ? error?.Message : null,
    });
}));

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
