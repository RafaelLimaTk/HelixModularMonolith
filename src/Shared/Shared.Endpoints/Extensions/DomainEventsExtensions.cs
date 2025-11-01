using Microsoft.Extensions.DependencyInjection;
using Shared.Application;
using Shared.Domain.SeedWorks;
using System.Reflection;

namespace Shared.Endpoints.Extensions;

public static class DomainEventsExtensions
{
    public static IServiceCollection AddDomainEventsWithAssemblies(
        this IServiceCollection services,
        params Assembly[] scanAssemblies)
    {
        services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();

        services.Scan(scan => scan
            .FromAssemblies(scanAssemblies)
            .AddClasses(c => c.AssignableTo(typeof(IDomainEventHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}
