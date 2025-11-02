using Helix.Chat.UnitTests.Application.Message.Common;

namespace Helix.Chat.UnitTests.Application.Message.MarkAsRead;

[CollectionDefinition(nameof(MarkAsReadTestFixture))]
public class MarkAsReadTestFixtureCollection
    : ICollectionFixture<MarkAsReadTestFixture>
{ }

public class MarkAsReadTestFixture
    : MessageUseCasesBaseFixture
{ }
