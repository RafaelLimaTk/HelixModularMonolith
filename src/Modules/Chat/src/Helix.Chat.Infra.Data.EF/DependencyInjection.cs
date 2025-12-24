using Helix.Chat.Infra.Data.EF.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infra.Outbox.Interfaces;

namespace Helix.Chat.Infra.Data.EF;

public static class DependencyInjection
{
    public static IServiceCollection AddChatInfraRegister(this IServiceCollection services)
    {
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();

        services.AddScoped<IOutboxStore, EfOutboxStore>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}