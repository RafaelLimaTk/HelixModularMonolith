using Helix.Chat.Query.Application.UseCases.ListMessagesByConversation;
using UseCase = Helix.Chat.Query.Application.UseCases.ListMessagesByConversation;

namespace Helix.Chat.UnitTests.Query.Application.Conversation.ListMessagesByConversation;

[Collection(nameof(ListMessagesByConversationTestFixture))]
public class ListMessagesByConversationTest(ListMessagesByConversationTestFixture fixture)
{
    private readonly ListMessagesByConversationTestFixture _fixture = fixture;

    [Fact(DisplayName = nameof(ListMessages))]
    [Trait("Chat/Application", "ListMessagesByConversation - Use Cases")]
    public async Task ListMessages()
    {
        var conversationId = _fixture.NewId();
        var repositoryMock = _fixture.GetMessagesReadRepositoryMock();
        var listMessages = _fixture.CreateExampleMessagesList(conversationId, length: 6);
        var expectedItems = listMessages.Where(m => m.ConversationId == conversationId).ToList();
        var input = new ListMessagesByConversationInput(conversationId, page: 1, perPage: 10);
        repositoryMock
            .Setup(x => x.Search(
                It.Is<IQuerySpecification<MessageQueryModel>>(spec =>
                    spec.Criteria != null &&
                    spec.Page == input.Page &&
                    spec.PerPage == input.PerPage),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchOutput<MessageQueryModel>(
                currentPage: 1,
                perPage: 10,
                total: listMessages.Count,
                items: expectedItems
            ));
        var useCase = new UseCase.ListMessagesByConversation(repositoryMock.Object);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(listMessages.Count);
        output.Items.Should().HaveCount(listMessages.Count);
        output.Items.ToList().ForEach(outputItem =>
        {
            var expectedItem = listMessages.FirstOrDefault(m => m.Id == outputItem.Id);
            expectedItem.Should().NotBeNull();
            outputItem.Should().NotBeNull();
            outputItem.Id.Should().Be(expectedItem!.Id);
            outputItem.ConversationId.Should().Be(expectedItem!.ConversationId);
            outputItem.SenderId.Should().Be(expectedItem.SenderId);
            outputItem.Content.Should().Be(expectedItem.Content);
            outputItem.SentAt.Should().Be(expectedItem.SentAt);
            outputItem.DeliveredAt.Should().Be(expectedItem.DeliveredAt);
            outputItem.ReadAt.Should().Be(expectedItem.ReadAt);
            outputItem.Status.Should().Be(expectedItem.Status);
        });
        repositoryMock.Verify(x => x.Search(
            It.Is<IQuerySpecification<MessageQueryModel>>(spec =>
                spec.Criteria != null &&
                spec.Page == input.Page &&
                spec.PerPage == input.PerPage &&
                expectedItems.All(e => spec.Criteria!.Compile()(e))
            ),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = nameof(ListEmpty))]
    [Trait("Chat/Application", "ListMessagesByConversation - Use Cases")]
    public async Task ListEmpty()
    {
        var conversationId = _fixture.NewId();
        var repositoryMock = _fixture.GetMessagesReadRepositoryMock();
        var input = new ListMessagesByConversationInput(conversationId, page: 1, perPage: 10);
        repositoryMock
            .Setup(x => x.Search(
                It.Is<IQuerySpecification<MessageQueryModel>>(spec =>
                    spec.Criteria != null &&
                    spec.Page == input.Page &&
                    spec.PerPage == input.PerPage),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchOutput<MessageQueryModel>(
                currentPage: 1,
                perPage: 10,
                total: 0,
                items: new List<MessageQueryModel>()
            ));
        var useCase = new UseCase.ListMessagesByConversation(
            repositoryMock.Object
        );

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(0);
        output.Items.Should().BeEmpty();
        repositoryMock.Verify(x => x.Search(
            It.Is<IQuerySpecification<MessageQueryModel>>(spec =>
                spec.Criteria != null &&
                spec.Page == input.Page &&
                spec.PerPage == input.PerPage),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory(DisplayName = nameof(ListReturnsPaginated))]
    [Trait("Chat/Application", "ListMessagesByConversation - Use Cases")]
    [InlineData(10, 1, 5, 5)]
    [InlineData(10, 2, 5, 5)]
    [InlineData(7, 2, 5, 2)]
    [InlineData(7, 3, 5, 0)]
    public async Task ListReturnsPaginated(
    int quantityMessagesToGenerate,
    int page,
    int perPage,
    int expectedQuantityItems)
    {
        Guid conversationId = _fixture.NewId();
        var repositoryMock = _fixture.GetMessagesReadRepositoryMock();
        var exampleMessagesList = _fixture.GetExampleMessagesList(conversationId, quantityMessagesToGenerate);
        repositoryMock
            .Setup(x => x.Search(
                It.IsAny<IQuerySpecification<MessageQueryModel>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IQuerySpecification<MessageQueryModel> spec, CancellationToken _) =>
            {
                var predicate = spec.Criteria?.Compile() ?? (Func<MessageQueryModel, bool>)(m => true);

                var filtered = exampleMessagesList
                    .Where(predicate)
                    .OrderByDescending(x => x.SentAt)
                    .ToList();

                var items = filtered
                    .Skip((spec.Page - 1) * spec.PerPage)
                    .Take(spec.PerPage)
                    .ToList();

                return new SearchOutput<MessageQueryModel>(spec.Page, spec.PerPage, filtered.Count, items);
            });

        var input = new ListMessagesByConversationInput(conversationId, page, perPage);
        var useCase = new UseCase.ListMessagesByConversation(repositoryMock.Object);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.Page.Should().Be(page);
        output.PerPage.Should().Be(perPage);
        output.Total.Should().Be(exampleMessagesList.Count);
        output.Items.Should().HaveCount(expectedQuantityItems);
        output.Items.ToList().ForEach(outputItem =>
        {
            var expectedItem = exampleMessagesList.FirstOrDefault(m => m.Id == outputItem.Id);
            expectedItem.Should().NotBeNull();
            outputItem.Should().NotBeNull();
            outputItem.Id.Should().Be(expectedItem!.Id);
            outputItem.ConversationId.Should().Be(expectedItem!.ConversationId);
            outputItem.SenderId.Should().Be(expectedItem.SenderId);
            outputItem.Content.Should().Be(expectedItem.Content);
            outputItem.SentAt.Should().Be(expectedItem.SentAt);
            outputItem.DeliveredAt.Should().Be(expectedItem.DeliveredAt);
            outputItem.ReadAt.Should().Be(expectedItem.ReadAt);
            outputItem.Status.Should().Be(expectedItem.Status);
        });
        repositoryMock.Verify(x => x.Search(
            It.Is<IQuerySpecification<MessageQueryModel>>(spec =>
                spec.Criteria != null &&
                spec.Page == input.Page &&
                spec.PerPage == input.PerPage),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory(DisplayName = nameof(ListSearchByText))]
    [Trait("Chat/Application", "ListMessagesByConversation - Use Cases")]
    [InlineData("hello")]
    [InlineData("TEST")]
    [InlineData("  test")]
    [InlineData("")]
    public async Task ListSearchByText(string searchText)
    {
        var conversationId = _fixture.NewId();
        var repositoryMock = _fixture.GetMessagesReadRepositoryMock();
        var listMessages = _fixture.CreateExampleMessagesList(conversationId, length: 10);
        var expectedItems = listMessages
            .Where(m => m.ConversationId == conversationId
                    && m.Content.Contains(searchText, StringComparison.InvariantCultureIgnoreCase))
            .ToList();
        var input = new ListMessagesByConversationInput(conversationId, page: 1, perPage: 10, search: searchText);
        repositoryMock
            .Setup(x => x.Search(
                It.Is<IQuerySpecification<MessageQueryModel>>(spec =>
                    spec.Criteria != null &&
                    spec.Page == (input.Page <= 0 ? 1 : input.Page) &&
                    spec.PerPage == (input.PerPage <= 0 ? 20 : Math.Min(input.PerPage, 100))),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IQuerySpecification<MessageQueryModel> spec, CancellationToken _) =>
            {
                var predicate = spec.Criteria?.Compile() ?? (Func<MessageQueryModel, bool>)(m => true);
                var filtered = listMessages.Where(predicate).ToList();
                return new SearchOutput<MessageQueryModel>(spec.Page, spec.PerPage, filtered.Count, filtered);
            });
        var useCase = new UseCase.ListMessagesByConversation(repositoryMock.Object);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(expectedItems.Count);
        output.Items.Should().HaveCount(expectedItems.Count);
        output.Items.ToList().ForEach(outputItem =>
        {
            var expectedItem = listMessages.FirstOrDefault(m => m.Id == outputItem.Id);
            expectedItem.Should().NotBeNull();
            outputItem.Should().NotBeNull();
            outputItem.Id.Should().Be(expectedItem!.Id);
            outputItem.ConversationId.Should().Be(expectedItem!.ConversationId);
            outputItem.SenderId.Should().Be(expectedItem.SenderId);
            outputItem.Content.Should().Be(expectedItem.Content);
            outputItem.SentAt.Should().Be(expectedItem.SentAt);
            outputItem.DeliveredAt.Should().Be(expectedItem.DeliveredAt);
            outputItem.ReadAt.Should().Be(expectedItem.ReadAt);
            outputItem.Status.Should().Be(expectedItem.Status);
        });
        repositoryMock.Verify(x => x.Search(
            It.Is<IQuerySpecification<MessageQueryModel>>(spec =>
                spec.Criteria != null &&
                spec.Page == input.Page &&
                spec.PerPage == input.PerPage &&
                expectedItems.All(e => spec.Criteria!.Compile()(e))
                ),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory(DisplayName = nameof(ListOrdered))]
    [Trait("Chat/Application", "ListMessagesByConversation - Use Cases")]
    [InlineData("sentAt", "asc")]
    [InlineData("sentAt", "desc")]
    [InlineData("status", "asc")]
    [InlineData("deliveredAt", "desc")]
    [InlineData("readAt", "asc")]
    [InlineData("readAt", "desc")]
    [InlineData("", "asc")]
    public async Task ListOrdered(
        string orderBy,
        string order)
    {
        var conversationId = _fixture.NewId();
        var repositoryMock = _fixture.GetMessagesReadRepositoryMock();
        var listMessages = _fixture.CreateExampleMessagesList(conversationId, length: 10);
        var expectedItems = listMessages.Where(m => m.ConversationId == conversationId).ToList();
        var useCaseOrder = order == "asc" ? SearchOrder.Asc : SearchOrder.Desc;
        var input = new ListMessagesByConversationInput(
            conversationId,
            page: 1,
            perPage: 10,
            sort: orderBy,
            dir: useCaseOrder
        );
        repositoryMock
            .Setup(x => x.Search(
                It.Is<IQuerySpecification<MessageQueryModel>>(spec =>
                    spec.Criteria != null &&
                    spec.Orders.Count == 1 &&
                    spec.Page == input.Page &&
                    spec.PerPage == input.PerPage),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IQuerySpecification<MessageQueryModel> spec, CancellationToken _) =>
            {
                var predicate = spec.Criteria?.Compile() ?? (Func<MessageQueryModel, bool>)(m => true);
                var order = spec.Orders.Single();
                var keySelector = order.KeySelector.Compile();

                var filtered = listMessages.Where(predicate);
                var ordered = order.Descending ? filtered.OrderByDescending(keySelector) : filtered.OrderBy(keySelector);
                var list = ordered.ToList();

                return new SearchOutput<MessageQueryModel>(spec.Page, spec.PerPage, list.Count, list);
            });
        var useCase = new UseCase.ListMessagesByConversation(repositoryMock.Object);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(listMessages.Count);
        output.Items.Should().HaveCount(listMessages.Count);
        repositoryMock.Verify(x => x.Search(
            It.Is<IQuerySpecification<MessageQueryModel>>(spec =>
                spec.Criteria != null &&
                spec.Page == input.Page &&
                spec.PerPage == input.PerPage &&
                expectedItems.All(e => spec.Criteria!.Compile()(e))
            ),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory(DisplayName = nameof(ListInputWithoutAllParameters))]
    [Trait("Chat/Application", "ListMessagesByConversation - Use Cases")]
    [MemberData(
        nameof(ListMessagesByConversationTestDataGenerator.GetInputsWithoutAllParameter),
        parameters: 14,
        MemberType = typeof(ListMessagesByConversationTestDataGenerator)
    )]
    public async Task ListInputWithoutAllParameters(ListMessagesByConversationInput input)
    {
        var conversationId = input.ConversationId;
        var repositoryMock = _fixture.GetMessagesReadRepositoryMock();
        var listMessages = _fixture.GetExampleMessagesList(conversationId, length: 6);
        var expectedItems = listMessages
            .Where(m => m.ConversationId == conversationId
                    && (string.IsNullOrWhiteSpace(input.Search)
                        || m.Content.Contains(input.Search, StringComparison.InvariantCultureIgnoreCase))
            ).ToList();
        repositoryMock
            .Setup(x => x.Search(
                It.Is<IQuerySpecification<MessageQueryModel>>(spec =>
                    spec.Criteria != null &&
                    spec.Page == (input.Page <= 0 ? 1 : input.Page) &&
                    spec.PerPage == (input.PerPage <= 0 ? 20 : Math.Min(input.PerPage, 100))),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchOutput<MessageQueryModel>(
                currentPage: input.Page <= 0 ? 1 : input.Page,
                perPage: input.PerPage <= 0 ? 20 : Math.Min(input.PerPage, 100),
                total: expectedItems.Count,
                items: expectedItems
            ));
        var useCase = new UseCase.ListMessagesByConversation(repositoryMock.Object);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(input.Page <= 0 ? 1 : input.Page);
        output.PerPage.Should().Be(input.PerPage <= 0 ? 20 : Math.Min(input.PerPage, 100));
        output.Total.Should().Be(expectedItems.Count);
        output.Items.Should().HaveCount(expectedItems.Count);
        output.Items.ToList().ForEach(outputItem =>
        {
            var expectedItem = listMessages.FirstOrDefault(m => m.Id == outputItem.Id);
            expectedItem.Should().NotBeNull();
            outputItem.Should().NotBeNull();
            outputItem.Id.Should().Be(expectedItem!.Id);
            outputItem.ConversationId.Should().Be(expectedItem!.ConversationId);
            outputItem.SenderId.Should().Be(expectedItem.SenderId);
            outputItem.Content.Should().Be(expectedItem.Content);
            outputItem.SentAt.Should().Be(expectedItem.SentAt);
            outputItem.DeliveredAt.Should().Be(expectedItem.DeliveredAt);
            outputItem.ReadAt.Should().Be(expectedItem.ReadAt);
            outputItem.Status.Should().Be(expectedItem.Status);
        });
        repositoryMock.Verify(x => x.Search(
            It.Is<IQuerySpecification<MessageQueryModel>>(spec =>
                spec.Criteria != null &&
                spec.Page == (input.Page <= 0 ? 1 : input.Page) &&
                spec.PerPage == (input.PerPage <= 0 ? 20 : Math.Min(input.PerPage, 100)) &&
                expectedItems.All(e => spec.Criteria!.Compile()(e))
            ),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
