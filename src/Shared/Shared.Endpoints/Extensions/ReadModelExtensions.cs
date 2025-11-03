using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Projections;
using Shared.Query.Interfaces;
using System.Reflection;

namespace Shared.Endpoints.Extensions;
public static class ReadModelExtensions
{
    public static IServiceCollection AddReadModelScanning(
        this IServiceCollection services,
        params Assembly[] scanAssemblies)
    {
        services.Scan(scan => scan
            .FromAssemblies(scanAssemblies)
            .AddClasses(c => c.AssignableTo<IReadDbConfiguration>())
                .AsImplementedInterfaces().WithSingletonLifetime()
            .AddClasses(c => c.AssignableTo(typeof(IProjectionHandler<>)))
                .AsImplementedInterfaces().WithScopedLifetime());

        return services;
    }
}
