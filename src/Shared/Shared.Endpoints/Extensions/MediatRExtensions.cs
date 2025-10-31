using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Shared.Endpoints.Extensions;
public static class MediatRExtensions
{
    public static IServiceCollection AddMediatRWithAssemblies
        (this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblies(assemblies);
        });

        return services;
    }
}
