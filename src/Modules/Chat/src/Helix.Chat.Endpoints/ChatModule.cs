using Helix.Chat.Query;
using Helix.Chat.Query.Data.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Endpoints.Extensions;

namespace Helix.Chat.Endpoints;
public static class ChatModule
{
    public static IServiceCollection AddChatModule(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MongoSettings>(options => configuration.GetSection("Mongo:Chat").Bind(options));

        services.AddMediatRWithAssemblies(ChatAssemblies.All);
        services.AddDomainEventsWithAssemblies(ChatAssemblies.All);
        services.AddReadModelScanning(ChatAssemblies.All);

        services.AddChatRegister();

        return services;
    }

    public static IApplicationBuilder UseChatModule(this IApplicationBuilder app)
    {
        return app;
    }
}
