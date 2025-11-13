using Bogus;
using Helix.Chat.Infra.Data.EF;
using Microsoft.EntityFrameworkCore;

namespace Helix.Chat.IntegrationTests.Base;
public class BaseFixture
{
    public BaseFixture()
        => Faker = new Faker("pt_BR");

    protected Faker Faker { get; set; }

    public HelixChatDbContext CreateDbContext(bool preserveData = false)
    {
        var context = new HelixChatDbContext(
            new DbContextOptionsBuilder<HelixChatDbContext>()
            .UseInMemoryDatabase("integration-tests-db")
            .Options
        );
        if (preserveData == false)
            context.Database.EnsureDeleted();
        return context;
    }
}
