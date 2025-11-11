using Helix.Chat.Query.Application.UseCases.ListUserConversations;
using Helix.Chat.Query.Models;
using Helix.Chat.UnitTests.Query.Application.Conversation.Common;
using Shared.Query.Interfaces.SearchableRepository;

namespace Helix.Chat.UnitTests.Query.Application.Conversation.ListUserConversations;

[CollectionDefinition(nameof(ListUserConversationsTestFixture))]
public class ListUserConversationsTestFixtureCollection
    : ICollectionFixture<ListUserConversationsTestFixture>
{ }

public class ListUserConversationsTestFixture : ConversationQueryUseCasesBaseFixture
{
    public List<ConversationQueryModel> GetExampleConversations(Guid userId, int length = 10)
        => Enumerable.Range(0, length)
            .Select(index =>
            {
                var title = index % 3 == 0 ? "Álgebra Linear" : index % 3 == 1 ? "Geometria" : "Cálculo";
                return CreateConversationQueryModel(userId, title);
            })
            .ToList();

    public ListUserConversationsInput GetExampleInput(Guid userId)
    {
        var random = Random.Shared;
        return new ListUserConversationsInput(
            userId: userId,
            page: random.Next(1, 5),
            perPage: random.Next(5, 50),
            search: Faker.Random.Bool() ? "algebra" : "",
            sort: Faker.Random.Bool() ? "title" : "updatedAt",
            dir: random.Next(0, 10) > 5 ? SearchOrder.Asc : SearchOrder.Desc
        );
    }
}