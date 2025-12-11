namespace Helix.Chat.IntegrationTests.Application.UseCases.Message.MarkAsRead;

[CollectionDefinition(nameof(MarkAsReadTestFixture))]
public class MarkAsReadTestFixtureCollection
    : ICollectionFixture<MarkAsReadTestFixture>
{ }

public class MarkAsReadTestFixture
    : MessageUseCasesBaseFixture
{
}
