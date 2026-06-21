using Microsoft.Extensions.DependencyInjection;

namespace Dotnetable.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
