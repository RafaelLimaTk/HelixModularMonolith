using Shared.Query.Specifications;

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
            new RepositoryRead.ConversationsReadOnlyRepository(_fixture.CreateReadDbContext(true));

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
        var exampleConversations = _fixture.GetExampleConversationsList(6);
        var dbContext = _fixture.CreateReadDbContext();
        var conversationsCollection = dbContext
            .GetCollection<ConversationQueryModel>(CollectionNames.Conversations);
        await conversationsCollection.InsertManyAsync(exampleConversations);
        var conversationRepository =
            new RepositoryRead.ConversationsReadOnlyRepository(_fixture.CreateReadDbContext(true));
        var input = new SearchInput(
            page: 1,
            perPage: 6,
            search: "",
            orderBy: "",
            order: SearchOrder.Desc);

        var conversations = await conversationRepository.Search(input, CancellationToken.None);

        conversations.Should().NotBeNull();
        conversations.CurrentPage.Should().Be(input.Page);
        conversations.PerPage.Should().Be(input.PerPage);
        conversations.Total.Should().Be(exampleConversations.Count);
        conversations.Items.Should().HaveCount(input.PerPage);
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

    [Fact(DisplayName = nameof(SearchReturnsEmptyWhenEmpty))]
    [Trait("Chat/Integration/Infra.Data", "ConversationReadOnlyRepository - Repositories")]
    public async Task SearchReturnsEmptyWhenEmpty()
    {
        IChatReadDbContext dbContext = _fixture.CreateReadDbContext();
        var conversationRepository =
            new RepositoryRead.ConversationsReadOnlyRepository(dbContext);
        var input = new SearchInput(
            page: 1,
            perPage: 6,
            search: "",
            orderBy: "",
            order: SearchOrder.Desc);

        var conversations = await conversationRepository.Search(input, CancellationToken.None);

        conversations.Should().NotBeNull();
        conversations.CurrentPage.Should().Be(input.Page);
        conversations.PerPage.Should().Be(input.PerPage);
        conversations.Total.Should().Be(0);
        conversations.Items.Should().HaveCount(0);
    }

    [Theory(DisplayName = nameof(SearchRetursPaginated))]
    [Trait("Chat/Integration/Infra.Data", "ConversationReadOnlyRepository - Repositories")]
    [InlineData(10, 1, 5, 5)]
    [InlineData(10, 2, 5, 5)]
    [InlineData(7, 2, 5, 2)]
    [InlineData(7, 3, 5, 0)]
    public async Task SearchRetursPaginated(
        int quantityConversationsToGenerate,
        int page,
        int perPage,
        int expectedQuantityItems)
    {
        var exampleConversations = _fixture.GetExampleConversationsList(quantityConversationsToGenerate);
        var dbContext = _fixture.CreateReadDbContext();
        var conversationsCollection = dbContext
            .GetCollection<ConversationQueryModel>(CollectionNames.Conversations);
        await conversationsCollection.InsertManyAsync(exampleConversations);
        var conversationRepository =
            new RepositoryRead.ConversationsReadOnlyRepository(_fixture.CreateReadDbContext(true));
        var input = new SearchInput(
            page: page,
            perPage: perPage,
            search: "",
            orderBy: "",
            order: SearchOrder.Desc);

        var conversations = await conversationRepository.Search(input, CancellationToken.None);

        conversations.Should().NotBeNull();
        conversations.CurrentPage.Should().Be(input.Page);
        conversations.PerPage.Should().Be(input.PerPage);
        conversations.Total.Should().Be(quantityConversationsToGenerate);
        conversations.Items.Should().HaveCount(expectedQuantityItems);
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

    [Theory(DisplayName = nameof(SearchByText))]
    [Trait("Chat/Integration/Infra.Data", "ConversationReadOnlyRepository - Repositories")]
    [InlineData("Support", 1, 5, 1, 1)]
    [InlineData("Questions", 1, 5, 2, 2)]
    [InlineData("Review", 2, 5, 0, 1)]
    [InlineData("Project", 1, 5, 4, 4)]
    [InlineData("Project", 1, 2, 2, 4)]
    [InlineData("Project", 2, 3, 1, 4)]
    [InlineData("Project Other", 1, 3, 0, 0)]
    [InlineData("Team", 1, 5, 2, 2)]
    public async Task SearchByText(
        string search,
        int page,
        int perPage,
        int expectedQuantityItems,
        int expectedTotalItems)
    {
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
        var exampleConversations = _fixture
            .GetExampleConversationsListByTitles(titles);
        var dbContext = _fixture.CreateReadDbContext();
        var conversationsCollection = dbContext
            .GetCollection<ConversationQueryModel>(CollectionNames.Conversations);
        await conversationsCollection.InsertManyAsync(exampleConversations);
        var conversationRepository =
            new RepositoryRead.ConversationsReadOnlyRepository(_fixture.CreateReadDbContext(true));
        var input = new SearchInput(
            page: page,
            perPage: perPage,
            search: search,
            orderBy: "",
            order: SearchOrder.Desc);

        var conversations = await conversationRepository.Search(input, CancellationToken.None);

        conversations.Should().NotBeNull();
        conversations.CurrentPage.Should().Be(input.Page);
        conversations.PerPage.Should().Be(input.PerPage);
        conversations.Total.Should().Be(expectedTotalItems);
        conversations.Items.Should().HaveCount(expectedQuantityItems);
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

    [Theory(DisplayName = nameof(SearchOrdered))]
    [Trait("Chat/Integration/Infra.Data", "ConversationReadOnlyRepository - Repositories")]
    [InlineData("title", "asc")]
    [InlineData("title", "desc")]
    [InlineData("createdAt", "asc")]
    [InlineData("createdAt", "desc")]
    [InlineData("updatedAt", "asc")]
    [InlineData("updatedAt", "desc")]
    [InlineData("", "asc")]
    public async Task SearchOrdered(string orderBy, string order)
    {
        var dbContext = _fixture.CreateReadDbContext();
        var exampleConversations = _fixture.GetExampleConversationsList(10);
        var conversationsCollection = dbContext
            .GetCollection<ConversationQueryModel>(CollectionNames.Conversations);
        await conversationsCollection.InsertManyAsync(exampleConversations);
        var conversationRepository =
            new RepositoryRead.ConversationsReadOnlyRepository(_fixture.CreateReadDbContext(true));
        var useCaseOrder = order == "asc" ? SearchOrder.Asc : SearchOrder.Desc;
        var input = new SearchInput(
            page: 1,
            perPage: 10,
            search: "",
            orderBy: orderBy,
            order: useCaseOrder);

        var conversations = await conversationRepository.Search(input, CancellationToken.None);

        var expectedOrderedList = _fixture.CloneConversationsListOrdered(
            exampleConversations,
            input.OrderBy,
            input.Order
        );
        conversations.Should().NotBeNull();
        conversations.Items.Should().NotBeNull();
        conversations.CurrentPage.Should().Be(input.Page);
        conversations.PerPage.Should().Be(input.PerPage);
        conversations.Total.Should().Be(exampleConversations.Count);
        conversations.Items.Should().HaveCount(exampleConversations.Count);
        for (int index = 0; index < expectedOrderedList.Count; index++)
        {
            var outputItem = conversations.Items[index];
            var exampleItem = expectedOrderedList[index];
            outputItem.Should().NotBeNull();
            exampleItem.Should().NotBeNull();
            outputItem.Id.Should().Be(exampleItem.Id);
            outputItem.Title.Should().Be(exampleItem.Title);
            outputItem.ParticipantIds.Should().BeEquivalentTo(exampleItem.ParticipantIds);
            outputItem.CreatedAt.Should().BeCloseTo(exampleItem.CreatedAt, TimeSpan.FromSeconds(1));
            outputItem.UpdatedAt.Should().BeCloseTo(exampleItem.UpdatedAt, TimeSpan.FromSeconds(1));
        }
    }

    [Fact(DisplayName = nameof(SearchBySpec))]
    [Trait("Chat/Integration/Infra.Data", "ConversationReadOnlyRepository - Repositories")]
    public async Task SearchBySpec()
    {
        var commonParticipants = _fixture.GetExampleParticipantsList();
        var exampleConversations = _fixture.GetExampleConversationsList(6, commonParticipants);
        var participantIdSpec = exampleConversations
            .SelectMany(c => c.ParticipantIds)
            .Intersect(commonParticipants)
            .First();
        var dbContext = _fixture.CreateReadDbContext();
        var conversationsCollection = dbContext
            .GetCollection<ConversationQueryModel>(CollectionNames.Conversations);
        await conversationsCollection.InsertManyAsync(exampleConversations);
        var conversationRepository =
            new RepositoryRead.ConversationsReadOnlyRepository(_fixture.CreateReadDbContext(true));
        var spec = new QuerySpecification<ConversationQueryModel>()
            .Where(c => c.ParticipantIds.Contains(participantIdSpec))
            .PageSize(1, 5)
            .OrderByDescending(c => c.CreatedAt);

        var conversations = await conversationRepository.Search(spec, CancellationToken.None);

        var expectedConversations = _fixture.FilterOrderAndPaginate(
            exampleConversations,
            predicate: c => c.ParticipantIds.Contains(participantIdSpec),
            page: spec.Page,
            perPage: spec.PerPage
        );
        var totalMatching = exampleConversations.Count(c => c.ParticipantIds.Contains(participantIdSpec));
        conversations.Should().NotBeNull();
        conversations.CurrentPage.Should().Be(spec.Page);
        conversations.PerPage.Should().Be(spec.PerPage);
        conversations.Total.Should().Be(totalMatching);
        conversations.Items.Should().HaveCount(expectedConversations.Count);
        conversations.Items.ToList().ForEach(conversation =>
        {
            var expectedConversation = expectedConversations
                .First(ec => ec.Id == conversation.Id);
            conversation.Title.Should().Be(expectedConversation.Title);
            conversation.ParticipantIds.Should().BeEquivalentTo(expectedConversation.ParticipantIds);
            conversation.CreatedAt.Should().BeCloseTo(expectedConversation.CreatedAt, TimeSpan.FromSeconds(1));
            conversation.UpdatedAt.Should().BeCloseTo(expectedConversation.UpdatedAt, TimeSpan.FromSeconds(1));
        });
    }

    [Fact(DisplayName = nameof(SearchBySpecReturnsEmptyWhenEmpty))]
    [Trait("Chat/Integration/Infra.Data", "ConversationReadOnlyRepository - Repositories")]
    public async Task SearchBySpecReturnsEmptyWhenEmpty()
    {
        IChatReadDbContext dbContext = _fixture.CreateReadDbContext();
        var conversationRepository =
            new RepositoryRead.ConversationsReadOnlyRepository(dbContext);
        var spec = new QuerySpecification<ConversationQueryModel>()
            .Where(c => c.ParticipantIds.Contains(Guid.NewGuid()))
            .PageSize(1, 5)
            .OrderByDescending(c => c.CreatedAt);

        var conversations = await conversationRepository.Search(spec, CancellationToken.None);

        conversations.Should().NotBeNull();
        conversations.CurrentPage.Should().Be(spec.Page);
        conversations.PerPage.Should().Be(spec.PerPage);
        conversations.Total.Should().Be(0);
        conversations.Items.Should().HaveCount(0);
    }
}
