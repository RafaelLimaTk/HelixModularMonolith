namespace Helix.Chat.IntegrationTests.Infra.Data.EF.OutboxStore;

[CollectionDefinition(nameof(EfOutboxStoreTestFixture))]
public class EfOutboxStoreTestFixtureCollection
    : ICollectionFixture<EfOutboxStoreTestFixture>
{ }

public class EfOutboxStoreTestFixture
    : BaseFixture
{
    public EventEnvelope CreateEnvelope()
    {
        var occurredOnUtc = DateTime.UtcNow;
        return new EventEnvelope(
            eventName: nameof(DomainEventFake),
            clrType: typeof(DomainEventFake).AssemblyQualifiedName!,
            payload: """{"value":"test"}""",
            occurredOnUtc: occurredOnUtc
        );
    }

    public OutboxMessage CreateOutboxMessage(bool processed = false)
    {
        var message = OutboxMessage.Create(
            type: typeof(DomainEventFake).AssemblyQualifiedName!,
            payload: Faker.Random.String2(10),
            occurredOnUtc: DateTime.UtcNow
        );

        if (processed)
        {
            message.MarkProcessed();
        }

        return message;
    }
}
