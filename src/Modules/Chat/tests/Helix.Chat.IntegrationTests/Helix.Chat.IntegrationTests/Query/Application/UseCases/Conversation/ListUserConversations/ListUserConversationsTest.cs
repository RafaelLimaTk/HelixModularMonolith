using Helix.Chat.Query.Application.UseCases.Conversation.ListUserConversations;
using UseCase = Helix.Chat.Query.Application.UseCases.Conversation.ListUserConversations;

namespace Helix.Chat.IntegrationTests.Query.Application.UseCases.Conversation.ListUserConversations;

[Collection(nameof(ListUserConversationsTestFixture))]
public class ListUserConversationsTest(ListUserConversationsTestFixture fixture)
{
    private readonly ListUserConversationsTestFixture _fixture = fixture;

    [Fact(DisplayName = nameof(ListConversations))]
    [Trait("Chat/Integration/Query/Application", "ListUserConversations - Use Cases")]
    public async Task ListConversations()
    {
        var dbContext = _fixture.CreateReadDbContext();
        var conversationsCollection = dbContext.GetCollection<ConversationQueryModel>(CollectionNames.Conversations);
        var userId = Guid.NewGuid();
        var titles = new List<string>
        {
            "Team Discussion",
            "Project Planning",
            "General Questions",
            "Support Chat",
            "Review Meeting"
        };
        var exampleConversations = _fixture.GetExampleConversationsListByTitles(titles, userId);
        await conversationsCollection.InsertManyAsync(exampleConversations);
        var repository = new RepositoryRead.ConversationsReadOnlyRepository(_fixture.CreateReadDbContext(true));
        var useCase = new UseCase.ListUserConversations(repository);
        var input = new ListUserConversationsInput(userId, page: 1, perPage: 10);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(exampleConversations.Count);
        output.Items.Should().HaveCount(exampleConversations.Count);
        foreach (var outputItem in output.Items)
        {
            var expectedItem = exampleConversations.FirstOrDefault(c => c.Id == outputItem.Id);
            expectedItem.Should().NotBeNull();
            outputItem.Title.Should().Be(expectedItem!.Title);
            outputItem.ParticipantIds.Should().BeEquivalentTo(expectedItem.ParticipantIds);
            outputItem.CreatedAt.Should().BeCloseTo(expectedItem.CreatedAt, TimeSpan.FromSeconds(1));
            outputItem.UpdatedAt.Should().BeCloseTo(expectedItem.UpdatedAt, TimeSpan.FromSeconds(1));
            if (expectedItem.LastMessage is not null)
            {
                outputItem.LastMessage.Should().NotBeNull();
                outputItem.LastMessage!.MessageId.Should().Be(expectedItem.LastMessage.MessageId);
                outputItem.LastMessage.Content.Should().Be(expectedItem.LastMessage.Content);
                outputItem.LastMessage.SentAt.Should().BeCloseTo(expectedItem.LastMessage.SentAt, TimeSpan.FromSeconds(1));
                outputItem.LastMessage.Status.Should().Be(expectedItem.LastMessage.Status);
            }
            else
            {
                outputItem.LastMessage.Should().BeNull();
            }
        }
    }

    [Theory(DisplayName = nameof(ListReturnsPaginated))]
    [Trait("Chat/Integration/Query/Application", "ListUserConversations - Use Cases")]
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
        int totalConversations,
        int page,
        int perPage,
        int expectedItemsCount)
    {
        var dbContext = _fixture.CreateReadDbContext();
        var conversationsCollection = dbContext.GetCollection<ConversationQueryModel>(CollectionNames.Conversations);
        var userId = Guid.NewGuid();
        var titles = Enumerable.Range(1, totalConversations)
            .Select(i => $"Conversation {i}")
            .ToList();
        var exampleConversations = _fixture.GetExampleConversationsListByTitles(titles, userId);
        await conversationsCollection.InsertManyAsync(exampleConversations);
        var repository = new RepositoryRead.ConversationsReadOnlyRepository(_fixture.CreateReadDbContext(true));
        var useCase = new UseCase.ListUserConversations(repository);
        var input = new ListUserConversationsInput(userId, page: page, perPage: perPage);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(totalConversations);
        output.Items.Should().HaveCount(expectedItemsCount);
        foreach (var outputItem in output.Items)
        {
            var expectedItem = exampleConversations.FirstOrDefault(c => c.Id == outputItem.Id);
            expectedItem.Should().NotBeNull();
            outputItem.Title.Should().Be(expectedItem!.Title);
            outputItem.ParticipantIds.Should().BeEquivalentTo(expectedItem.ParticipantIds);
            outputItem.CreatedAt.Should().BeCloseTo(expectedItem.CreatedAt, TimeSpan.FromSeconds(1));
            outputItem.UpdatedAt.Should().BeCloseTo(expectedItem.UpdatedAt, TimeSpan.FromSeconds(1));
        }
    }

    [Theory(DisplayName = nameof(ListInputWithoutAllParameters))]
    [Trait("Chat/Integration/Query/Application", "ListUserConversations - Use Cases")]
    [InlineData(0, 0)]
    [InlineData(-1, -1)]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    public async Task ListInputWithoutAllParameters(int inputPage, int inputPerPage)
    {
        var dbContext = _fixture.CreateReadDbContext();
        var conversationsCollection = dbContext.GetCollection<ConversationQueryModel>(CollectionNames.Conversations);
        var userId = Guid.NewGuid();
        var titles = new List<string> { "Test Conversation" };
        var exampleConversations = _fixture.GetExampleConversationsListByTitles(titles, userId);
        await conversationsCollection.InsertManyAsync(exampleConversations);
        var repository = new RepositoryRead.ConversationsReadOnlyRepository(_fixture.CreateReadDbContext(true));
        var useCase = new UseCase.ListUserConversations(repository);
        var input = new ListUserConversationsInput(userId, page: inputPage, perPage: inputPerPage);

        var output = await useCase.Handle(input, CancellationToken.None);

        var expectedPage = inputPage <= 0 ? 1 : inputPage;
        var expectedPerPage = inputPerPage <= 0 ? 20 : Math.Min(inputPerPage, 100);
        output.Should().NotBeNull();
        output.Page.Should().Be(expectedPage);
        output.PerPage.Should().Be(expectedPerPage);
    }

    [Theory(DisplayName = nameof(SearchByText))]
    [Trait("Chat/Integration/Query/Application", "ListUserConversations - Use Cases")]
    [InlineData("Project", 1, 5, 4, 4)]
    [InlineData("Project", 1, 2, 2, 4)]
    [InlineData("Project", 2, 2, 2, 4)]
    [InlineData("Project", 3, 2, 0, 4)]
    [InlineData("project", 1, 5, 4, 4)]
    [InlineData("PROJECT", 1, 5, 4, 4)]
    [InlineData("Team", 1, 5, 2, 2)]
    [InlineData("Questions", 1, 5, 2, 2)]
    [InlineData("Support", 1, 5, 1, 1)]
    [InlineData("nonexistent", 1, 5, 0, 0)]
    [InlineData("", 1, 10, 9, 9)]
    public async Task SearchByText(
        string search,
        int page,
        int perPage,
        int expectedItemsCount,
        int expectedTotal)
    {
        var dbContext = _fixture.CreateReadDbContext();
        var conversationsCollection = dbContext.GetCollection<ConversationQueryModel>(CollectionNames.Conversations);
        var userId = Guid.NewGuid();
        var titles = new List<string>
        {
            "Project Discussion",
            "Team Meeting",
            "General Questions",
            "Support Chat",
            "Project Review",
            "Questions and Answers",
            "Project Planning",
            "Team Outing",
            "Project updates"
        };
        var exampleConversations = _fixture.GetExampleConversationsListByTitles(titles, userId);
        await conversationsCollection.InsertManyAsync(exampleConversations);
        var repository = new RepositoryRead.ConversationsReadOnlyRepository(_fixture.CreateReadDbContext(true));
        var useCase = new UseCase.ListUserConversations(repository);
        var input = new ListUserConversationsInput(userId, page: page, perPage: perPage, search: search);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(expectedTotal);
        output.Items.Should().HaveCount(expectedItemsCount);
        foreach (var outputItem in output.Items)
        {
            var expectedItem = exampleConversations.FirstOrDefault(c => c.Id == outputItem.Id);
            expectedItem.Should().NotBeNull();
            outputItem.Title.Should().Be(expectedItem!.Title);
            outputItem.ParticipantIds.Should().BeEquivalentTo(expectedItem.ParticipantIds);
            outputItem.CreatedAt.Should().BeCloseTo(expectedItem.CreatedAt, TimeSpan.FromSeconds(1));
            outputItem.UpdatedAt.Should().BeCloseTo(expectedItem.UpdatedAt, TimeSpan.FromSeconds(1));
        }
    }

    [Theory(DisplayName = nameof(ListOrdered))]
    [Trait("Chat/Integration/Query/Application", "ListUserConversations - Use Cases")]
    [InlineData("title", "asc")]
    [InlineData("title", "desc")]
    [InlineData("createdAt", "asc")]
    [InlineData("createdAt", "desc")]
    [InlineData("updatedAt", "asc")]
    [InlineData("updatedAt", "desc")]
    [InlineData("", "desc")]
    public async Task ListOrdered(string sort, string direction)
    {
        var dbContext = _fixture.CreateReadDbContext();
        var conversationsCollection = dbContext.GetCollection<ConversationQueryModel>(CollectionNames.Conversations);
        var userId = Guid.NewGuid();
        var titles = new List<string>
        {
            "Alpha Project",
            "Beta Testing",
            "Gamma Release",
            "Delta Updates",
            "Epsilon Meeting"
        };
        var exampleConversations = _fixture.GetExampleConversationsListByTitles(titles, userId);
        await conversationsCollection.InsertManyAsync(exampleConversations);
        var repository = new RepositoryRead.ConversationsReadOnlyRepository(_fixture.CreateReadDbContext(true));
        var useCase = new UseCase.ListUserConversations(repository);
        var searchOrder = direction == "asc" ? SearchOrder.Asc : SearchOrder.Desc;
        var input = new ListUserConversationsInput(
            userId,
            page: 1,
            perPage: 10,
            search: "",
            sort: sort,
            dir: searchOrder);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Total.Should().Be(exampleConversations.Count);
        output.Items.Should().HaveCount(exampleConversations.Count);
        var expectedOrderedList = _fixture.CloneConversationsListOrdered(
            exampleConversations,
            sort,
            searchOrder);
        for (int index = 0; index < output.Items.Count; index++)
        {
            var outputItem = output.Items.ElementAt(index);
            var expectedItem = expectedOrderedList[index];
            outputItem.Id.Should().Be(expectedItem.Id);
            outputItem.Title.Should().Be(expectedItem.Title);
            outputItem.ParticipantIds.Should().BeEquivalentTo(expectedItem.ParticipantIds);
            outputItem.CreatedAt.Should().BeCloseTo(expectedItem.CreatedAt, TimeSpan.FromSeconds(1));
            outputItem.UpdatedAt.Should().BeCloseTo(expectedItem.UpdatedAt, TimeSpan.FromSeconds(1));
        }
    }
}
