using Asp.Versioning;

namespace Dotnetable.API.Versioning;

public static class ApiVersioningExtensions
{
    /// <summary>
    /// Header-based versioning: clients send <c>X-Api-Version</c>. Missing header = 1.0 so
    /// existing storefronts keep working. Breaking changes are a new version on the same path.
    /// </summary>
    public static IApiVersioningBuilder AddDotnetableApiVersioning(this IServiceCollection services)
    {
        return services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = ApiVersions.V1;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new HeaderApiVersionReader(ApiVersions.HeaderName);
            options.UnsupportedApiVersionStatusCode = StatusCodes.Status400BadRequest;
        })
        .AddMvc(options => options.Conventions.Add(new ImplicitCurrentApiVersionConvention()));
    }

    /// <summary>Echoes the negotiated version on the response as <c>X-Api-Version</c>.</summary>
    public static IApplicationBuilder UseDotnetableApiVersionHeaders(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            context.Response.OnStarting(() =>
            {
                var version = context.Features.Get<IApiVersioningFeature>()?.RequestedApiVersion;
                if (version is not null && !context.Response.Headers.ContainsKey(ApiVersions.HeaderName))
                    context.Response.Headers[ApiVersions.HeaderName] = version.ToString();
                return Task.CompletedTask;
            });
            await next();
        });
    }
}
