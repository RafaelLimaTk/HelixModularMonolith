namespace Helix.Chat.UnitTests.Query.Common;

[CollectionDefinition(nameof(QueryBaseFixture))]
public class QueryFixtureCollection
    : ICollectionFixture<QueryBaseFixture>
{ }

public class QueryBaseFixture : BaseFixture
{
    public Guid NewId() => Guid.NewGuid();
    public string AnyContent() => Faker.Lorem.Sentence();
}