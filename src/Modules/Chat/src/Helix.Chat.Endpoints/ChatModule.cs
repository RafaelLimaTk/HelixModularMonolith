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
        services.AddMediatRWithAssemblies(ChatAssemblies.All);
        return services;
    }

    public static IApplicationBuilder UseChatModule(this IApplicationBuilder app)
    {
        return app;
    }
}
