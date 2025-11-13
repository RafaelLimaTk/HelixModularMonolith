using Microsoft.Extensions.DependencyInjection;
using Shared.Infra.Outbox;
using Shared.Infra.Outbox.Interfaces;

namespace Helix.Chat.Infra.Data.EF;
public static class DependencyInjection
{
    public static IServiceCollection AddChatInfraRegister(this IServiceCollection services)
    {
        services.AddScoped<IOutboxStore, EfOutboxStore>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}