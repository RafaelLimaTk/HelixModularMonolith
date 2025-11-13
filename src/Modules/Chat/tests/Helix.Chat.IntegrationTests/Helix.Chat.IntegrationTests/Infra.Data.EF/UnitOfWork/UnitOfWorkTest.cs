using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UnitOfWorkInfra = Helix.Chat.Infra.Data.EF;

namespace Helix.Chat.IntegrationTests.Infra.Data.EF.UnitOfWork;

[Collection(nameof(UnitOfWorkTestFixture))]
public class UnitOfWorkTest(UnitOfWorkTestFixture fixture)
{
    private readonly UnitOfWorkTestFixture _fixture = fixture;
    private readonly Mock<IOutboxStore> _outboxStoreMock = new();

    [Fact(DisplayName = nameof(Commit))]
    [Trait("Chat/Integration/Infra.Data", "UnitOfWork - Persistence")]
    public async Task Commit()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleConversationsList = _fixture.GetExampleConversationsList();
        var conversationWithEvent = exampleConversationsList.First();
        var @event = new DomainEventFake();
        conversationWithEvent.RaiseEvent(@event);
        await dbContext.AddRangeAsync(exampleConversationsList);
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var unitOfWork = new UnitOfWorkInfra.UnitOfWork(
            dbContext,
            _outboxStoreMock.Object,
            serviceProvider.GetRequiredService<ILogger<UnitOfWorkInfra.UnitOfWork>>()
        );

        await unitOfWork.Commit(CancellationToken.None);

        var assertDbContext = _fixture.CreateDbContext(true);
        var savedConversations = assertDbContext.Conversations
            .AsNoTracking().ToList();
        savedConversations.Should()
            .HaveCount(exampleConversationsList.Count);
        _outboxStoreMock.Verify(x => x.AppendAsync(
                It.Is<EventEnvelope>(e =>
                    e.EventName == typeof(DomainEventFake).Name &&
                    e.ClrType == typeof(DomainEventFake).AssemblyQualifiedName &&
                    !string.IsNullOrWhiteSpace(e.Payload)),
                It.IsAny<CancellationToken>()),
            Times.Once);
        conversationWithEvent.Events.Should().BeEmpty();
    }

    [Fact(DisplayName = nameof(Rollback))]
    [Trait("Chat/Integration/Infra.Data", "UnitOfWork - Persistence")]
    public async Task Rollback()
    {
        var dbContext = _fixture.CreateDbContext();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var unitOfWork = new UnitOfWorkInfra.UnitOfWork(
            dbContext,
            _outboxStoreMock.Object,
            serviceProvider.GetRequiredService<ILogger<UnitOfWorkInfra.UnitOfWork>>()
        );

        var task = async ()
            => await unitOfWork.Rollback(CancellationToken.None);

        await task.Should().NotThrowAsync();
    }
}
