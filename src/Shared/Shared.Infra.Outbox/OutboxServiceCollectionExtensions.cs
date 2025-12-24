using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Infra.Outbox.Configuration;
using Shared.Infra.Outbox.Interfaces;
using Shared.Infra.Outbox.Services;
using System.Reflection;

namespace Shared.Infra.Outbox;

public static class OutboxServiceCollectionExtensions
{
    public static IServiceCollection AddOutboxProcessor(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<OutboxProcessorOptions>? configureOptions = null,
        params Assembly[] assemblies)
    {
        var options = new OutboxProcessorOptions();
        configuration.GetSection("Outbox")?.Bind(options);
        configureOptions?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<ITypeResolver>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<TypeResolver>>();
            var resolver = new TypeResolver(logger);
            foreach (var assembly in assemblies)
            {
                resolver.RegisterAssembly(assembly);
            }
            return resolver;
        });
        services.AddSingleton<OutboxProcessor>();
        services.AddSingleton<IOutboxProcessor>(sp => sp.GetRequiredService<OutboxProcessor>());
        services.AddHostedService(sp => sp.GetRequiredService<OutboxProcessor>());

        return services;
    }

    public static IServiceCollection AddOutboxProcessor(
        this IServiceCollection services,
        Action<OutboxProcessorOptions> configureOptions,
        params Assembly[] assemblies)
    {
        var options = new OutboxProcessorOptions();
        configureOptions(options);

        services.AddSingleton(options);
        services.AddSingleton<ITypeResolver>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<TypeResolver>>();
            var resolver = new TypeResolver(logger);
            foreach (var assembly in assemblies)
            {
                resolver.RegisterAssembly(assembly);
            }
            return resolver;
        });
        services.AddSingleton<OutboxProcessor>();
        services.AddSingleton<IOutboxProcessor>(sp => sp.GetRequiredService<OutboxProcessor>());
        services.AddHostedService(sp => sp.GetRequiredService<OutboxProcessor>());

        return services;
    }
}