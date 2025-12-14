using Helix.Chat.Query.Application.UseCases.Conversation.ListMessagesByConversation;
using UseCase = Helix.Chat.Query.Application.UseCases.Conversation.ListMessagesByConversation;

namespace Helix.Chat.IntegrationTests.Query.Application.UseCases.Conversation.ListMessagesByConversation;

[Collection(nameof(ListMessagesByConversationTestFixture))]
public class ListMessagesByConversationTest(ListMessagesByConversationTestFixture fixture)
{
    private readonly ListMessagesByConversationTestFixture _fixture = fixture;

    [Fact(DisplayName = nameof(ListMessages))]
    [Trait("Chat/Integration/Query/Application", "ListMessagesByConversation - Use Cases")]
    public async Task ListMessages()
    {
        var dbContext = _fixture.CreateReadDbContext();
        var messagesCollection = dbContext.GetCollection<MessageQueryModel>(CollectionNames.Messages);
        var conversationId = Guid.NewGuid();
        var exampleMessages = _fixture.GetExampleMessagesListByContents(conversationId);
        await messagesCollection.InsertManyAsync(exampleMessages);
        var repository = new RepositoryRead.MessagesReadOnlyRepository(_fixture.CreateReadDbContext(true));
        var useCase = new UseCase.ListMessagesByConversation(repository);
        var input = new ListMessagesByConversationInput(conversationId, page: 1, perPage: 10);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(exampleMessages.Count);
        output.Items.Should().HaveCount(exampleMessages.Count);
        foreach (var outputItem in output.Items)
        {
            var expectedItem = exampleMessages.FirstOrDefault(m => m.Id == outputItem.Id);
            expectedItem.Should().NotBeNull();
            outputItem.ConversationId.Should().Be(expectedItem!.ConversationId);
            outputItem.SenderId.Should().Be(expectedItem.SenderId);
            outputItem.Content.Should().Be(expectedItem.Content);
            outputItem.SentAt.Should().BeCloseTo(expectedItem.SentAt, TimeSpan.FromSeconds(1));
            outputItem.Status.Should().Be(expectedItem.Status);
        }
    }

    [Fact(DisplayName = nameof(ListEmpty))]
    [Trait("Chat/Integration/Query/Application", "ListMessagesByConversation - Use Cases")]
    public async Task ListEmpty()
    {
        var conversationId = Guid.NewGuid();
        var repository = new RepositoryRead.MessagesReadOnlyRepository(_fixture.CreateReadDbContext(true));
        var useCase = new UseCase.ListMessagesByConversation(repository);
        var input = new ListMessagesByConversationInput(conversationId, page: 1, perPage: 10);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(0);
        output.Items.Should().BeEmpty();
    }

    [Theory(DisplayName = nameof(ListWithInvalidPaginationParameters))]
    [Trait("Chat/Integration/Query/Application", "ListMessagesByConversation - Use Cases")]
    [InlineData(-1, 10, 1, 10)]
    [InlineData(0, 10, 1, 10)]
    [InlineData(1, -1, 1, 20)]
    [InlineData(1, 0, 1, 20)]
    [InlineData(1, 200, 1, 100)]
    [InlineData(-5, -10, 1, 20)]
    public async Task ListWithInvalidPaginationParameters(
        int inputPage,
        int inputPerPage,
        int expectedPage,
        int expectedPerPage)
    {
        var dbContext = _fixture.CreateReadDbContext();
        var messagesCollection = dbContext.GetCollection<MessageQueryModel>(CollectionNames.Messages);
        var conversationId = Guid.NewGuid();
        var exampleMessages = _fixture.GetExampleMessagesListByContents(conversationId);
        await messagesCollection.InsertManyAsync(exampleMessages);
        var repository = new RepositoryRead.MessagesReadOnlyRepository(_fixture.CreateReadDbContext(true));
        var useCase = new UseCase.ListMessagesByConversation(repository);
        var input = new ListMessagesByConversationInput(conversationId, page: inputPage, perPage: inputPerPage);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(expectedPage);
        output.PerPage.Should().Be(expectedPerPage);
    }

    [Theory(DisplayName = nameof(SearchByText))]
    [Trait("Chat/Integration/Query/Application", "ListMessagesByConversation - Use Cases")]
    [InlineData("hello", 1, 5, 3, 3)]
    [InlineData("hello", 2, 5, 0, 3)]
    [InlineData("world", 1, 5, 2, 2)]
    [InlineData("HELLO", 1, 5, 3, 3)]
    [InlineData("important", 1, 5, 1, 1)]
    [InlineData("test", 1, 5, 4, 4)]
    [InlineData("test", 1, 2, 2, 4)]
    [InlineData("test", 2, 2, 2, 4)]
    [InlineData("test", 3, 2, 0, 4)]
    [InlineData("nonexistent", 1, 5, 0, 0)]
    [InlineData("", 1, 10, 10, 10)]
    public async Task SearchByText(
        string search,
        int page,
        int perPage,
        int expectedItemsCount,
        int expectedTotal)
    {
        var dbContext = _fixture.CreateReadDbContext();
        var messagesCollection = dbContext.GetCollection<MessageQueryModel>(CollectionNames.Messages);
        var conversationId = Guid.NewGuid();
        var contents = new List<string>
        {
            "Hello John, did you receive the files?",
            "Hello team, good morning — welcome to the update.",
            "Hello — please review the latest changes before the smoke test.",
            "The world health report is out.",
            "This is an important update about the release.",
            "I'm testing the new notification flow right now.",
            "We should run a performance test this evening.",
            "Around the world users are reporting intermittent errors.",
            "Can we schedule a call to discuss the integration?",
            "Final compatibility test completed successfully."
        };
        var exampleMessages = _fixture.GetExampleMessagesListByContents(conversationId, contents);
        await messagesCollection.InsertManyAsync(exampleMessages);
        var repository = new RepositoryRead.MessagesReadOnlyRepository(_fixture.CreateReadDbContext(true));
        var useCase = new UseCase.ListMessagesByConversation(repository);
        var input = new ListMessagesByConversationInput(conversationId, page: page, perPage: perPage, search: search);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(expectedTotal);
        output.Items.Should().HaveCount(expectedItemsCount);
        foreach (var outputItem in output.Items)
        {
            var expectedItem = exampleMessages.FirstOrDefault(m => m.Id == outputItem.Id);
            expectedItem.Should().NotBeNull();
            outputItem.ConversationId.Should().Be(expectedItem!.ConversationId);
            outputItem.SenderId.Should().Be(expectedItem.SenderId);
            outputItem.Content.Should().Be(expectedItem.Content);
            outputItem.SentAt.Should().BeCloseTo(expectedItem.SentAt, TimeSpan.FromSeconds(1));
            outputItem.Status.Should().Be(expectedItem.Status);
        }
    }

    [Theory(DisplayName = nameof(ListOrdered))]
    [Trait("Chat/Integration/Query/Application", "ListMessagesByConversation - Use Cases")]
    [InlineData("sentAt", "asc")]
    [InlineData("sentAt", "desc")]
    [InlineData("status", "asc")]
    [InlineData("status", "desc")]
    [InlineData("deliveredAt", "asc")]
    [InlineData("deliveredAt", "desc")]
    [InlineData("readAt", "asc")]
    [InlineData("readAt", "desc")]
    [InlineData("", "desc")]
    public async Task ListOrdered(string sort, string direction)
    {
        var dbContext = _fixture.CreateReadDbContext();
        var messagesCollection = dbContext.GetCollection<MessageQueryModel>(CollectionNames.Messages);
        var conversationId = Guid.NewGuid();
        var exampleMessages = _fixture.GetExampleMessagesListByContents(conversationId);
        await messagesCollection.InsertManyAsync(exampleMessages);
        var repository = new RepositoryRead.MessagesReadOnlyRepository(_fixture.CreateReadDbContext(true));
        var useCase = new UseCase.ListMessagesByConversation(repository);
        var searchOrder = direction == "asc" ? SearchOrder.Asc : SearchOrder.Desc;
        var input = new ListMessagesByConversationInput(
            conversationId,
            page: 1,
            perPage: 10,
            search: "",
            sort: sort,
            dir: searchOrder);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Total.Should().Be(exampleMessages.Count);
        output.Items.Should().HaveCount(exampleMessages.Count);
        var expectedOrderedList = _fixture.CloneMessagesListOrdered(
            exampleMessages,
            sort,
            searchOrder);
        for (int i = 0; i < output.Items.Count; i++)
        {
            var outputItem = output.Items.ElementAt(i);
            var expectedItem = expectedOrderedList[i];
            outputItem.Id.Should().Be(expectedItem.Id);
            outputItem.ConversationId.Should().Be(expectedItem!.ConversationId);
            outputItem.SenderId.Should().Be(expectedItem.SenderId);
            outputItem.Content.Should().Be(expectedItem.Content);
            outputItem.SentAt.Should().BeCloseTo(expectedItem.SentAt, TimeSpan.FromSeconds(1));
            outputItem.Status.Should().Be(expectedItem.Status);
        }
    }

    [Theory(DisplayName = nameof(ListReturnsPaginated))]
    [Trait("Chat/Integration/Query/Application", "ListMessagesByConversation - Use Cases")]
    [InlineData(10, 1, 5, 5)]
    [InlineData(10, 2, 5, 5)]
    [InlineData(10, 3, 5, 0)]
    [InlineData(7, 1, 5, 5)]
    [InlineData(7, 2, 5, 2)]
    [InlineData(7, 3, 5, 0)]
    [InlineData(15, 1, 10, 10)]
    [InlineData(15, 2, 10, 5)]
    [InlineData(3, 1, 10, 3)]
    public async Task ListReturnsPaginated(
        int totalMessages,
        int page,
        int perPage,
        int expectedItemsCount)
    {
        var dbContext = _fixture.CreateReadDbContext();
        var messagesCollection = dbContext.GetCollection<MessageQueryModel>(CollectionNames.Messages);
        var conversationId = Guid.NewGuid();
        var exampleMessages = _fixture.CreateExampleMessagesList(conversationId, totalMessages);
        await messagesCollection.InsertManyAsync(exampleMessages);
        var repository = new RepositoryRead.MessagesReadOnlyRepository(_fixture.CreateReadDbContext(true));
        var useCase = new UseCase.ListMessagesByConversation(repository);
        var input = new ListMessagesByConversationInput(conversationId, page: page, perPage: perPage);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(page);
        output.PerPage.Should().Be(perPage);
        output.Total.Should().Be(totalMessages);
        output.Items.Should().HaveCount(expectedItemsCount);
        var expectedItems = exampleMessages
            .OrderByDescending(m => m.SentAt)
            .ThenByDescending(m => m.Id)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToList();
        output.Items.Should().HaveCount(expectedItems.Count);
        foreach (var outputItem in output.Items)
        {
            var expectedItem = exampleMessages.FirstOrDefault(m => m.Id == outputItem.Id);
            expectedItem.Should().NotBeNull();
            outputItem.ConversationId.Should().Be(expectedItem!.ConversationId);
            outputItem.SenderId.Should().Be(expectedItem.SenderId);
            outputItem.Content.Should().Be(expectedItem.Content);
            outputItem.SentAt.Should().BeCloseTo(expectedItem.SentAt, TimeSpan.FromSeconds(1));
            outputItem.Status.Should().Be(expectedItem.Status);
        }
    }
}
