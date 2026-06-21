using Asp.Versioning;
using Dotnetable.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

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

builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.ContentRootPath);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
