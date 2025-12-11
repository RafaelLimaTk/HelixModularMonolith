namespace Helix.Chat.IntegrationTests.Query.Repositories.MessagesReadOnlyRepository;

[Collection(nameof(MessagesReadOnlyRepositoryTestFixture))]
public class MessagesReadOnlyRepositoryTest(MessagesReadOnlyRepositoryTestFixture fixture)
{
    private readonly MessagesReadOnlyRepositoryTestFixture _fixture = fixture;

    [Fact(DisplayName = nameof(Get))]
    [Trait("Chat/Integration/Infra.Data", "MessagesReadOnlyRepository - Repositories")]
    public async Task Get()
    {
        var exampleMessage = _fixture.GetExampleMessage();
        var dbContext = _fixture.CreateReadDbContext();
        var messagesCollection = dbContext
            .GetCollection<MessageQueryModel>(CollectionNames.Messages);
        await messagesCollection.InsertOneAsync(exampleMessage);
        var messageRepository =
            new RepositoryRead.MessagesReadOnlyRepository(_fixture.CreateReadDbContext(true));

        var message = await messageRepository.Get(exampleMessage.Id, CancellationToken.None);

        message.Should().NotBeNull();
        message.Id.Should().Be(exampleMessage.Id);
        message.ConversationId.Should().Be(exampleMessage.ConversationId);
        message.SenderId.Should().Be(exampleMessage.SenderId);
        message.Content.Should().Be(exampleMessage.Content);
        message.SentAt.Should().BeCloseTo(exampleMessage.SentAt, TimeSpan.FromSeconds(1));

        if (exampleMessage.DeliveredAt.HasValue)
            message.DeliveredAt.Should().BeCloseTo(exampleMessage.DeliveredAt.Value, TimeSpan.FromSeconds(1));
        else
            message.DeliveredAt.Should().BeNull();

        if (exampleMessage.ReadAt.HasValue)
            message.ReadAt.Should().BeCloseTo(exampleMessage.ReadAt.Value, TimeSpan.FromSeconds(1));
        else
            message.ReadAt.Should().BeNull();
    }

    [Fact(DisplayName = nameof(Search))]
    [Trait("Chat/Integration/Infra.Data", "MessagesReadOnlyRepository - Repositories")]
    public async Task Search()
    {
        var exampleMessages = _fixture.GetExampleMessagesList(6);
        var dbContext = _fixture.CreateReadDbContext();
        var messagesCollection = dbContext
            .GetCollection<MessageQueryModel>(CollectionNames.Messages);
        await messagesCollection.InsertManyAsync(exampleMessages);
        var messageRepository =
            new RepositoryRead.MessagesReadOnlyRepository(_fixture.CreateReadDbContext(true));
        var input = new SearchInput(
            page: 1,
            perPage: 6,
            search: "",
            orderBy: "",
            order: SearchOrder.Desc);

        var messages = await messageRepository.Search(input, CancellationToken.None);

        messages.Should().NotBeNull();
        messages.CurrentPage.Should().Be(input.Page);
        messages.PerPage.Should().Be(input.PerPage);
        messages.Total.Should().Be(exampleMessages.Count);
        messages.Items.Should().HaveCount(input.PerPage);
        messages.Items.ToList().ForEach(message =>
        {
            var expectedMessage = exampleMessages.First(em => em.Id == message.Id);
            message.ConversationId.Should().Be(expectedMessage.ConversationId);
            message.SenderId.Should().Be(expectedMessage.SenderId);
            message.Content.Should().Be(expectedMessage.Content);
            message.SentAt.Should().BeCloseTo(expectedMessage.SentAt, TimeSpan.FromSeconds(1));
        });
    }

    [Fact(DisplayName = nameof(SearchReturnsEmptyWhenEmpty))]
    [Trait("Chat/Integration/Infra.Data", "MessagesReadOnlyRepository - Repositories")]
    public async Task SearchReturnsEmptyWhenEmpty()
    {
        IChatReadDbContext dbContext = _fixture.CreateReadDbContext();
        var messageRepository = new RepositoryRead.MessagesReadOnlyRepository(dbContext);
        var input = new SearchInput(
            page: 1,
            perPage: 6,
            search: "",
            orderBy: "",
            order: SearchOrder.Desc);

        var messages = await messageRepository.Search(input, CancellationToken.None);

        messages.Should().NotBeNull();
        messages.CurrentPage.Should().Be(input.Page);
        messages.PerPage.Should().Be(input.PerPage);
        messages.Total.Should().Be(0);
        messages.Items.Should().HaveCount(0);
    }

    [Theory(DisplayName = nameof(SearchReturnsPaginated))]
    [Trait("Chat/Integration/Infra.Data", "MessagesReadOnlyRepository - Repositories")]
    [InlineData(10, 1, 5, 5)]
    [InlineData(10, 2, 5, 5)]
    [InlineData(7, 2, 5, 2)]
    [InlineData(7, 3, 5, 0)]
    public async Task SearchReturnsPaginated(
        int quantityMessagesToGenerate,
        int page,
        int perPage,
        int expectedQuantityItems)
    {
        var exampleMessages = _fixture.GetExampleMessagesList(quantityMessagesToGenerate);
        var dbContext = _fixture.CreateReadDbContext();
        var messagesCollection = dbContext
            .GetCollection<MessageQueryModel>(CollectionNames.Messages);
        await messagesCollection.InsertManyAsync(exampleMessages);
        var messageRepository =
            new RepositoryRead.MessagesReadOnlyRepository(_fixture.CreateReadDbContext(true));
        var input = new SearchInput(
            page: page,
            perPage: perPage,
            search: "",
            orderBy: "",
            order: SearchOrder.Desc);

        var messages = await messageRepository.Search(input, CancellationToken.None);

        messages.Should().NotBeNull();
        messages.CurrentPage.Should().Be(input.Page);
        messages.PerPage.Should().Be(input.PerPage);
        messages.Total.Should().Be(quantityMessagesToGenerate);
        messages.Items.Should().HaveCount(expectedQuantityItems);
    }

    [Theory(DisplayName = nameof(SearchByText))]
    [Trait("Chat/Integration/Infra.Data", "MessagesReadOnlyRepository - Repositories")]
    [InlineData("hello", 1, 5, 2, 2)]
    [InlineData("world", 1, 5, 2, 2)]
    [InlineData("important", 1, 5, 3, 3)]
    [InlineData("meeting", 1, 2, 2, 2)]
    [InlineData("update", 1, 5, 2, 2)]
    [InlineData("xyz", 1, 5, 0, 0)]
    public async Task SearchByText(
        string search,
        int page,
        int perPage,
        int expectedQuantityItems,
        int expectedTotalItems)
    {
        var contents = new List<string>
        {
            "Hello everyone",
            "Important message here",
            "Meeting scheduled for tomorrow",
            "World news update",
            "Another important note",
            "Meeting reminder",
            "Status update",
            "Hello world",
            "Important announcement"
        };
        var exampleMessages = _fixture.GetExampleMessagesListByContent(contents);
        var dbContext = _fixture.CreateReadDbContext();
        var messagesCollection = dbContext
            .GetCollection<MessageQueryModel>(CollectionNames.Messages);
        await messagesCollection.InsertManyAsync(exampleMessages);
        var messageRepository =
            new RepositoryRead.MessagesReadOnlyRepository(_fixture.CreateReadDbContext(true));
        var input = new SearchInput(
            page: page,
            perPage: perPage,
            search: search,
            orderBy: "",
            order: SearchOrder.Desc);

        var messages = await messageRepository.Search(input, CancellationToken.None);

        messages.Should().NotBeNull();
        messages.CurrentPage.Should().Be(input.Page);
        messages.PerPage.Should().Be(input.PerPage);
        messages.Total.Should().Be(expectedTotalItems);
        messages.Items.Should().HaveCount(expectedQuantityItems);
    }

    [Theory(DisplayName = nameof(SearchOrdered))]
    [Trait("Chat/Integration/Infra.Data", "MessagesReadOnlyRepository - Repositories")]
    [InlineData("sentAt", "asc")]
    [InlineData("sentAt", "desc")]
    [InlineData("deliveredAt", "asc")]
    [InlineData("deliveredAt", "desc")]
    [InlineData("readAt", "asc")]
    [InlineData("readAt", "desc")]
    [InlineData("", "asc")]
    public async Task SearchOrdered(string orderBy, string order)
    {
        var dbContext = _fixture.CreateReadDbContext();
        var exampleMessages = _fixture.GetExampleMessagesList(10);
        var messagesCollection = dbContext
            .GetCollection<MessageQueryModel>(CollectionNames.Messages);
        await messagesCollection.InsertManyAsync(exampleMessages);
        var messageRepository =
            new RepositoryRead.MessagesReadOnlyRepository(_fixture.CreateReadDbContext(true));
        var useCaseOrder = order == "asc" ? SearchOrder.Asc : SearchOrder.Desc;
        var input = new SearchInput(
            page: 1,
            perPage: 10,
            search: "",
            orderBy: orderBy,
            order: useCaseOrder);

        var messages = await messageRepository.Search(input, CancellationToken.None);

        var expectedOrderedList = _fixture.CloneMessagesListOrdered(
            exampleMessages,
            input.OrderBy,
            input.Order);

        messages.Should().NotBeNull();
        messages.Items.Should().NotBeNull();
        messages.CurrentPage.Should().Be(input.Page);
        messages.PerPage.Should().Be(input.PerPage);
        messages.Total.Should().Be(exampleMessages.Count);
        messages.Items.Should().HaveCount(exampleMessages.Count);

        for (int index = 0; index < expectedOrderedList.Count; index++)
        {
            var outputItem = messages.Items[index];
            var exampleItem = expectedOrderedList[index];
            outputItem.Should().NotBeNull();
            exampleItem.Should().NotBeNull();
            outputItem.Id.Should().Be(exampleItem.Id);
            outputItem.Content.Should().Be(exampleItem.Content);
            outputItem.SentAt.Should().BeCloseTo(exampleItem.SentAt, TimeSpan.FromSeconds(1));
        }
    }

    [Fact(DisplayName = nameof(SearchBySpec))]
    [Trait("Chat/Integration/Infra.Data", "MessagesReadOnlyRepository - Repositories")]
    public async Task SearchBySpec()
    {
        var conversationId = Guid.NewGuid();
        var exampleMessages = _fixture.GetExampleMessagesListByConversation(conversationId, 6);
        var dbContext = _fixture.CreateReadDbContext();
        var messagesCollection = dbContext
            .GetCollection<MessageQueryModel>(CollectionNames.Messages);
        await messagesCollection.InsertManyAsync(exampleMessages);
        var messageRepository =
            new RepositoryRead.MessagesReadOnlyRepository(_fixture.CreateReadDbContext(true));
        var spec = new QuerySpecification<MessageQueryModel>()
            .Where(m => m.ConversationId == conversationId)
            .PageSize(1, 5)
            .OrderBy(m => m.SentAt);

        var messages = await messageRepository.Search(spec, CancellationToken.None);

        var expectedMessages = _fixture.FilterOrderAndPaginate(
            exampleMessages,
            predicate: m => m.ConversationId == conversationId,
            orderBy: "sentAt",
            order: SearchOrder.Asc,
            page: spec.Page,
            perPage: spec.PerPage);

        messages.Should().NotBeNull();
        messages.CurrentPage.Should().Be(spec.Page);
        messages.PerPage.Should().Be(spec.PerPage);
        messages.Total.Should().Be(exampleMessages.Count);
        messages.Items.Should().HaveCount(expectedMessages.Count);

        for (int i = 0; i < messages.Items.Count; i++)
        {
            messages.Items[i].Id.Should().Be(expectedMessages[i].Id);
            messages.Items[i].Content.Should().Be(expectedMessages[i].Content);
        }
    }

    [Fact(DisplayName = nameof(SearchBySpecReturnsEmptyWhenEmpty))]
    [Trait("Chat/Integration/Infra.Data", "MessagesReadOnlyRepository - Repositories")]
    public async Task SearchBySpecReturnsEmptyWhenEmpty()
    {
        IChatReadDbContext dbContext = _fixture.CreateReadDbContext();
        var messageRepository = new RepositoryRead.MessagesReadOnlyRepository(dbContext);
        var spec = new QuerySpecification<MessageQueryModel>()
            .Where(m => m.ConversationId == Guid.NewGuid())
            .PageSize(1, 5)
            .OrderBy(m => m.SentAt);

        var messages = await messageRepository.Search(spec, CancellationToken.None);

        messages.Should().NotBeNull();
        messages.CurrentPage.Should().Be(spec.Page);
        messages.PerPage.Should().Be(spec.PerPage);
        messages.Total.Should().Be(0);
        messages.Items.Should().HaveCount(0);
    }

    [Theory(DisplayName = nameof(SearchBySpecReturnsPaginated))]
    [Trait("Chat/Integration/Infra.Data", "MessagesReadOnlyRepository - Repositories")]
    [InlineData(10, 1, 5)]
    [InlineData(10, 2, 5)]
    [InlineData(7, 2, 5)]
    [InlineData(7, 3, 5)]
    public async Task SearchBySpecReturnsPaginated(
        int quantityMessagesToGenerate,
        int page,
        int perPage)
    {
        var conversationId = Guid.NewGuid();
        var exampleMessages = _fixture
            .GetExampleMessagesListByConversation(conversationId, quantityMessagesToGenerate);
        var dbContext = _fixture.CreateReadDbContext();
        var messagesCollection = dbContext
            .GetCollection<MessageQueryModel>(CollectionNames.Messages);
        await messagesCollection.InsertManyAsync(exampleMessages);
        var messageRepository =
            new RepositoryRead.MessagesReadOnlyRepository(_fixture.CreateReadDbContext(true));
        var spec = new QuerySpecification<MessageQueryModel>()
            .Where(m => m.ConversationId == conversationId)
            .PageSize(page, perPage)
            .OrderBy(m => m.SentAt);

        var messages = await messageRepository.Search(spec, CancellationToken.None);

        var expectedMessages = _fixture.FilterOrderAndPaginate(
            exampleMessages,
            predicate: m => m.ConversationId == conversationId,
            orderBy: "sentAt",
            order: SearchOrder.Asc,
            page: spec.Page,
            perPage: spec.PerPage);

        messages.Should().NotBeNull();
        messages.CurrentPage.Should().Be(spec.Page);
        messages.PerPage.Should().Be(spec.PerPage);
        messages.Total.Should().Be(exampleMessages.Count);
        messages.Items.Should().HaveCount(expectedMessages.Count);
    }

    [Theory(DisplayName = nameof(SearchBySpecAndText))]
    [Trait("Chat/Integration/Infra.Data", "MessagesReadOnlyRepository - Repositories")]
    [InlineData("hello", 1, 5, 2, 2)]
    [InlineData("important", 1, 5, 3, 3)]
    [InlineData("meeting", 1, 2, 2, 2)]
    [InlineData("xyz", 1, 5, 0, 0)]
    public async Task SearchBySpecAndText(
        string search,
        int page,
        int perPage,
        int expectedQuantityItems,
        int expectedTotalItems)
    {
        var contents = new List<string>
        {
            "Hello everyone",
            "Important message here",
            "Meeting scheduled",
            "Another important note",
            "Meeting reminder",
            "Hello world",
            "Important announcement"
        };
        var exampleMessages = _fixture.GetExampleMessagesListByContent(contents);
        var dbContext = _fixture.CreateReadDbContext();
        var messagesCollection = dbContext
            .GetCollection<MessageQueryModel>(CollectionNames.Messages);
        await messagesCollection.InsertManyAsync(exampleMessages);
        var messageRepository =
            new RepositoryRead.MessagesReadOnlyRepository(_fixture.CreateReadDbContext(true));
        var spec = new QuerySpecification<MessageQueryModel>()
            .Where(m => m.Content.Contains(search, StringComparison.CurrentCultureIgnoreCase))
            .PageSize(page, perPage)
            .OrderBy(m => m.SentAt);

        var messages = await messageRepository.Search(spec, CancellationToken.None);

        messages.Should().NotBeNull();
        messages.CurrentPage.Should().Be(spec.Page);
        messages.PerPage.Should().Be(spec.PerPage);
        messages.Total.Should().Be(expectedTotalItems);
        messages.Items.Should().HaveCount(expectedQuantityItems);
    }

    [Theory(DisplayName = nameof(SearchBySpecOrdered))]
    [Trait("Chat/Integration/Infra.Data", "MessagesReadOnlyRepository - Repositories")]
    [InlineData("sentAt", "asc")]
    [InlineData("sentAt", "desc")]
    [InlineData("deliveredAt", "asc")]
    [InlineData("deliveredAt", "desc")]
    [InlineData("readAt", "asc")]
    [InlineData("readAt", "desc")]
    public async Task SearchBySpecOrdered(string orderBy, string order)
    {
        var conversationId = Guid.NewGuid();
        var exampleMessages = _fixture.GetExampleMessagesListByConversation(conversationId, 10);
        var dbContext = _fixture.CreateReadDbContext();
        var messagesCollection = dbContext
            .GetCollection<MessageQueryModel>(CollectionNames.Messages);
        await messagesCollection.InsertManyAsync(exampleMessages);
        var messageRepository =
            new RepositoryRead.MessagesReadOnlyRepository(_fixture.CreateReadDbContext(true));
        var useCaseOrder = order == "asc" ? SearchOrder.Asc : SearchOrder.Desc;
        var spec = _fixture.BuildSpecificationForConversation(
            conversationId,
            orderBy,
            useCaseOrder,
            page: 1,
            perPage: 10);

        var messages = await messageRepository.Search(spec, CancellationToken.None);

        var expectedOrdered = _fixture.CloneMessagesListOrdered(
            exampleMessages,
            orderBy,
            useCaseOrder);

        messages.Should().NotBeNull();
        messages.CurrentPage.Should().Be(spec.Page);
        messages.PerPage.Should().Be(spec.PerPage);
        messages.Total.Should().Be(exampleMessages.Count);
        messages.Items.Should().HaveCount(expectedOrdered.Count);

        for (int index = 0; index < expectedOrdered.Count; index++)
        {
            var outputItem = messages.Items[index];
            var expectedItem = expectedOrdered[index];
            outputItem.Id.Should().Be(expectedItem.Id);
            outputItem.Content.Should().Be(expectedItem.Content);
            outputItem.SentAt.Should().BeCloseTo(expectedItem.SentAt, TimeSpan.FromSeconds(1));
        }
    }
}
