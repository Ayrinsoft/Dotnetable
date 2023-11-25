using Asp.Versioning;
using Blazored.LocalStorage;
using Dotnetable.Admin.Components;
using Dotnetable.Admin.SharedServices;
using Dotnetable.Admin.SharedServices.Authorization;
using Dotnetable.Admin.SharedServices.Data;
using Dotnetable.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;
using MudBlazor;
using MudBlazor.Services;
using System.Globalization;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthorizationCore();

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopRight;

    config.SnackbarConfiguration.PreventDuplicates = true;
    config.SnackbarConfiguration.NewestOnTop = true;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 8000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazorContextMenu();


builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ApiVersionReader = new HeaderApiVersionReader("api-version");
});


builder.Services.AddLocalization();
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();

builder.Services.AddControllers(opt => { opt.Filters.Add(new AutoLogger()); })
                .ConfigureApiBehaviorOptions(options => { options.InvalidModelStateResponseFactory = ModelStateValidator.ValidateModelState; })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                });

#region JWT&AUTH
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = true;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(LocalSecret.TokenHashKey(builder.Configuration["AdminPanelSettings:ClientHash"]))),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddSingleton<IAuthorizationPolicyProvider, APIAuthorizationPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, APIAuthorizationHandler>();
#endregion


var cultureInfo = new CultureInfo("en-US");
CultureInfo.CurrentCulture = cultureInfo;
CultureInfo.CurrentUICulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

builder.Services.AddScoped<CustomAuthentication>();
builder.Services.AddScoped<AuthenticationStateProvider>(s => s.GetRequiredService<CustomAuthentication>());
builder.Services.AddHttpClient<IHttpServices, HttpServices>();
builder.Services.AddSingleton<HttpClient>();
builder.Services.AddHttpContextAccessor();


builder.Services.AddSingleton<MemberService>();
builder.Services.AddSingleton<PlaceService>();
builder.Services.AddSingleton<MessageService>();
builder.Services.AddSingleton<AuthenticationService>();
builder.Services.AddSingleton<FileService>();
builder.Services.AddSingleton<PostService>();
builder.Services.AddSingleton<LogsService>();
builder.Services.AddSingleton<CommentService>();
builder.Services.AddSingleton<WebsiteService>();

var app = builder.Build();

app.UseResponseCaching();
app.UseStaticFiles(); // new StaticFileOptions() { OnPrepareResponse = ctx => { ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=2592000"); } });

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
