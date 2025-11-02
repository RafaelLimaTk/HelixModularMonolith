using Helix.Chat.UnitTests.Application.Message.Common;

namespace Helix.Chat.UnitTests.Application.Message.MarkAsDelivered;

[CollectionDefinition(nameof(MarkAsDeliveredTestFixture))]
public class MarkAsDeliveredTestFixtureCollection
    : ICollectionFixture<MarkAsDeliveredTestFixture>
{ }

public class MarkAsDeliveredTestFixture : MessageUseCasesBaseFixture
{ }