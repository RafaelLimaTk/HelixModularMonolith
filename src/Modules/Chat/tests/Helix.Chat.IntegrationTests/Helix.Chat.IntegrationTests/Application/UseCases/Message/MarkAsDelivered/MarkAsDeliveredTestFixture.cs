namespace Helix.Chat.IntegrationTests.Application.UseCases.Message.MarkAsDelivered;

[CollectionDefinition(nameof(MarkAsDeliveredTestFixture))]
public class MarkAsDeliveredTestFixtureCollection
    : ICollectionFixture<MarkAsDeliveredTestFixture>
{ }

public class MarkAsDeliveredTestFixture
    : MessageUseCasesBaseFixture
{
}
