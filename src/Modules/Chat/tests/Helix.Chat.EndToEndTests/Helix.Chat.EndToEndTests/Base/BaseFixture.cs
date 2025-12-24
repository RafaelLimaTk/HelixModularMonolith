using Bogus;
using Helix.Api;
using Microsoft.Extensions.Configuration;
using Shared.Infra.Outbox.Interfaces;

namespace Helix.Chat.EndToEndTests.Base;

public class BaseFixture
{
    protected Faker Faker { get; set; }
    public CustomWebApplicationFactory<IApiMarker> WebAppFactory { get; set; }
    public HttpClient HttpClient { get; set; }
    public ApiClient ApiClient { get; set; }
    private readonly string? _dbConnectionString;

    public BaseFixture()
    {
        Faker = new Faker("pt_BR");
        WebAppFactory = new CustomWebApplicationFactory<IApiMarker>();
        HttpClient = WebAppFactory.CreateClient();
        ApiClient = new ApiClient(HttpClient);
        var configuration = WebAppFactory.Services.GetService(typeof(IConfiguration));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
        _dbConnectionString = ((IConfiguration)configuration).GetConnectionString("ChatDatabase");
    }

    public HelixChatDbContext CreateDbContext(bool preserveData = false)
    {
        var context = new HelixChatDbContext(
            new DbContextOptionsBuilder<HelixChatDbContext>()
                .UseNpgsql(_dbConnectionString, npgsql =>
                {
                    npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "chat");
                    npgsql.EnableRetryOnFailure();
                })
                .UseSnakeCaseNamingConvention()
                .Options
        );
        return context;
    }

    public void CleanPersistence()
    {
        var context = CreateDbContext();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }

    public async Task ProcessOutboxAsync(CancellationToken cancellationToken = default)
    {
        var processor = WebAppFactory.Services.GetRequiredService<IOutboxProcessor>();
        await processor.ProcessPendingAsync(cancellationToken);
    }
}
