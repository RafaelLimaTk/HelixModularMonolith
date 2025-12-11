using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infra.Outbox.Configuration;
using Shared.Infra.Outbox.Interfaces;
using Shared.Infra.Outbox.Services;

namespace Shared.Infra.Outbox;

public static class OutboxServiceCollectionExtensions
{
    public static IServiceCollection AddOutboxProcessor(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<OutboxProcessorOptions>? configureOptions = null)
    {
        var options = new OutboxProcessorOptions();
        configuration.GetSection("Outbox")?.Bind(options);
        configureOptions?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<ITypeResolver, TypeResolver>();
        services.AddHostedService<OutboxProcessor>();

        return services;
    }

    public static IServiceCollection AddOutboxProcessor(
        this IServiceCollection services,
        Action<OutboxProcessorOptions> configureOptions)
    {
        var options = new OutboxProcessorOptions();
        configureOptions(options);

        services.AddSingleton(options);
        services.AddSingleton<ITypeResolver, TypeResolver>();
        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}