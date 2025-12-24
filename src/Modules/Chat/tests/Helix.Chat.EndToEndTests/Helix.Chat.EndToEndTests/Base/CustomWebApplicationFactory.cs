using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Shared.Infra.Outbox.Configuration;

namespace Helix.Chat.EndToEndTests.Base;

public class CustomWebApplicationFactory<TStartup>
    : WebApplicationFactory<TStartup>, IDisposable
    where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("EndToEndTest");
        builder.ConfigureServices(services =>
        {
            var outboxOptionsDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(OutboxProcessorOptions));

            if (outboxOptionsDescriptor != null)
            {
                services.Remove(outboxOptionsDescriptor);
            }

            services.AddSingleton(new OutboxProcessorOptions { Enabled = false });

            var serviceProvider = services.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<HelixChatDbContext>();

            if (db is null) throw new ArgumentNullException(nameof(db));

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        });

        base.ConfigureWebHost(builder);
    }
}