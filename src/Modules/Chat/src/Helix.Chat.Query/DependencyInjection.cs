using Helix.Chat.Query.Data.Context;
using Helix.Chat.Query.Data.Repositories;
using Helix.Chat.Query.Data.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Shared.Query.Interfaces;

namespace Helix.Chat.Query;
public static class DependencyInjection
{
    public static IServiceCollection AddChatRegister(this IServiceCollection services)
    {
        services.AddSingleton<IChatReadDbContext, NoSqlDbContext>();
        services.AddSingleton<ISynchronizeDb, MongoSynchronizeDb>();

        services.AddScoped<IConversationsReadRepository, ConversationsReadOnlyRepository>();
        services.AddScoped<IMessagesReadRepository, MessagesReadOnlyRepository>();

        return services;
    }
}