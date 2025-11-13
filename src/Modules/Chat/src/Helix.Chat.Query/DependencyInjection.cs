using Helix.Chat.Query.Data.Context;
using Helix.Chat.Query.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Helix.Chat.Query;
public static class DependencyInjection
{
    public static IServiceCollection AddChatQueryRegister(this IServiceCollection services)
    {
        services.AddSingleton<IChatReadDbContext, NoSqlDbContext>();
        services.AddSingleton<ISynchronizeDb, MongoSynchronizeDb>();

        services.AddScoped<IConversationsReadRepository, ConversationsReadOnlyRepository>();
        services.AddScoped<IMessagesReadRepository, MessagesReadOnlyRepository>();

        return services;
    }
}