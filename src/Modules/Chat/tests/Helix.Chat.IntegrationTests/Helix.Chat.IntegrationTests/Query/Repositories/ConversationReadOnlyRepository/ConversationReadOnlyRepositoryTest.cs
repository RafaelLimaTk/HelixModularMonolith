using Helix.Chat.Query.Data;
using Helix.Chat.Query.Models;
using Shared.Query.Interfaces.SearchableRepository;

namespace Helix.Chat.IntegrationTests.Query.Repositories.ConversationReadOnlyRepository;

[Collection(nameof(ConversationReadOnlyRepositoryTestFixture))]
public class ConversationReadOnlyRepositoryTest(ConversationReadOnlyRepositoryTestFixture fixture)
{
    private readonly ConversationReadOnlyRepositoryTestFixture _fixture = fixture;

    [Fact(DisplayName = nameof(Get))]
    [Trait("Chat/Integration/Infra.Data", "ConversationReadOnlyRepository - Repositories")]
    public async Task Get()
    {
        var exampleConversation = _fixture.GetExampleConversation();
        var dbContext = _fixture.CreateReadDbContext();
        var conversationsCollection = dbContext
            .GetCollection<ConversationQueryModel>(CollectionNames.Conversations);
        await conversationsCollection.InsertOneAsync(exampleConversation);
        var conversationRepository =
            new RepositoryRead.ConversationsReadOnlyRepository(_fixture.CreateReadDbContext());

        var conversation = await conversationRepository
            .Get(exampleConversation.Id, CancellationToken.None);

        conversation.Should().NotBeNull();
        conversation.Id.Should().Be(exampleConversation.Id);
        conversation.Title.Should().Be(exampleConversation.Title);
        conversation.ParticipantIds.Should().BeEquivalentTo(exampleConversation.ParticipantIds);
        conversation.CreatedAt.Should().BeCloseTo(exampleConversation.CreatedAt, TimeSpan.FromSeconds(1));
        conversation.UpdatedAt.Should().BeCloseTo(exampleConversation.UpdatedAt, TimeSpan.FromSeconds(1));
    }

    [Fact(DisplayName = nameof(Search))]
    [Trait("Chat/Integration/Infra.Data", "ConversationReadOnlyRepository - Repositories")]
    public async Task Search()
    {
        var exampleConversations = _fixture.GetExampleConversationsList(5);
        var dbContext = _fixture.CreateReadDbContext();
        var conversationsCollection = dbContext
            .GetCollection<ConversationQueryModel>(CollectionNames.Conversations);
        await conversationsCollection.InsertManyAsync(exampleConversations);
        var conversationRepository =
            new RepositoryRead.ConversationsReadOnlyRepository(_fixture.CreateReadDbContext());
        var input = new SearchInput(
            page: 1,
            perPage: 10,
            search: "",
            orderBy: "",
            order: SearchOrder.Desc);

        var conversations = await conversationRepository.Search(input, CancellationToken.None);

        conversations.Should().NotBeNull();
        conversations.CurrentPage.Should().Be(input.Page);
        conversations.PerPage.Should().Be(input.PerPage);
        conversations.Total.Should().Be(exampleConversations.Count);
        conversations.Items.Should().HaveCount(input.PerPage);
        conversations.Items.Should().BeInDescendingOrder(c => c.CreatedAt);
        conversations.Items.ToList().ForEach(conversation =>
        {
            var expectedConversation = exampleConversations
                .First(ec => ec.Id == conversation.Id);
            conversation.Title.Should().Be(expectedConversation.Title);
            conversation.ParticipantIds.Should().BeEquivalentTo(expectedConversation.ParticipantIds);
            conversation.CreatedAt.Should().BeCloseTo(expectedConversation.CreatedAt, TimeSpan.FromSeconds(1));
            conversation.UpdatedAt.Should().BeCloseTo(expectedConversation.UpdatedAt, TimeSpan.FromSeconds(1));
        });
    }
}
