using Helix.Chat.Infra.Data.EF;
using Helix.Chat.Query;
using Helix.Chat.Query.Data.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Endpoints.Extensions;

namespace Helix.Chat.Endpoints;
public static class ChatModule
{
    public static IServiceCollection AddChatModule(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");
        services.AddDbContext<HelixChatDbContext>(options =>
            options
                .UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "chat");
                    npgsql.EnableRetryOnFailure();
                })
                .UseSnakeCaseNamingConvention()
        );

        services.Configure<MongoSettings>(options => configuration.GetSection("Mongo:Chat").Bind(options));

        services.AddMediatRWithAssemblies(ChatAssemblies.All);
        services.AddDomainEventsWithAssemblies(ChatAssemblies.All);
        services.AddReadModelScanning(ChatAssemblies.All);

        services.AddChatQueryRegister();
        services.AddChatInfraRegister();

        return services;
    }

    public static IApplicationBuilder UseChatModule(this IApplicationBuilder app)
    {
        return app;
    }
}
