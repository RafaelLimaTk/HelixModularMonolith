using Helix.Chat.Query.Application.UseCases.Conversation.ListUserConversations;
using Helix.Chat.UnitTests.Query.Application.Conversation.Common;
using UseCase = Helix.Chat.Query.Application.UseCases.Conversation.ListUserConversations;

namespace Helix.Chat.UnitTests.Query.Application.Conversation.ListUserConversations;

public class ListUserConversationsTest : ConversationQueryUseCasesBaseFixture
{
    [Fact(DisplayName = nameof(ListConversations))]
    [Trait("Chat/Application", "ListUserConversations - Use Cases")]
    public async Task ListConversations()
    {
        var userId = NewId();
        var repositoryMock = GetConversationReadRepositoryMock();
        var listConversations = CreateExampleConversationsList(userId, 5);
        var expectedItems = listConversations.Where(c => c.ParticipantIds.Contains(userId)).ToList();
        var input = new ListUserConversationsInput(userId, page: 1, perPage: 10, "");
        repositoryMock
            .Setup(x => x.Search(
                It.Is<IQuerySpecification<ConversationQueryModel>>(spec =>
                    spec.Criteria != null &&
                    spec.Page == input.Page &&
                    spec.PerPage == input.PerPage),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchOutput<ConversationQueryModel>(
                currentPage: 1,
                perPage: 10,
                total: listConversations.Count,
                items: expectedItems
            ));
        var useCase = new UseCase.ListUserConversations(
            repositoryMock.Object
        );

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(listConversations.Count);
        output.Items.Should().HaveCount(listConversations.Count);
        output.Items.ToList().ForEach(outputItem =>
        {
            var expectedItem = listConversations.FirstOrDefault(c => c.Id == outputItem.Id);
            expectedItem.Should().NotBeNull();
            outputItem.Should().NotBeNull();
            outputItem.Id.Should().Be(expectedItem!.Id);
            outputItem.Title.Should().Be(expectedItem!.Title);
            outputItem.CreatedAt.Should().Be(expectedItem.CreatedAt);
            outputItem.UpdatedAt.Should().Be(expectedItem.UpdatedAt);
            outputItem.ParticipantIds.Should().BeEquivalentTo(expectedItem.ParticipantIds);
            if (expectedItem.LastMessage is not null)
            {
                outputItem.LastMessage.Should().NotBeNull();
                outputItem.LastMessage!.MessageId.Should().Be(expectedItem.LastMessage.MessageId);
                outputItem.LastMessage.Content.Should().Be(expectedItem.LastMessage.Content);
                outputItem.LastMessage.SentAt.Should().Be(expectedItem.LastMessage.SentAt);
                outputItem.LastMessage.Status.Should().Be(expectedItem.LastMessage.Status);
            }
            else
            {
                outputItem.LastMessage.Should().BeNull();
            }
        });

        repositoryMock.Verify(x => x.Search(
            It.Is<IQuerySpecification<ConversationQueryModel>>(spec =>
                spec.Criteria != null &&
                spec.Page == input.Page &&
                spec.PerPage == input.PerPage &&
                expectedItems.All(e => spec.Criteria!.Compile()(e))
            ),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = nameof(ListEmpty))]
    [Trait("Chat/Application", "ListUserConversations - Use Cases")]
    public async Task ListEmpty()
    {
        var userId = NewId();
        var repositoryMock = GetConversationReadRepositoryMock();
        var input = new ListUserConversationsInput(userId, page: 1, perPage: 10, "");
        repositoryMock
            .Setup(x => x.Search(
                It.Is<IQuerySpecification<ConversationQueryModel>>(spec =>
                    spec.Criteria != null &&
                    spec.Page == input.Page &&
                    spec.PerPage == input.PerPage),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchOutput<ConversationQueryModel>(
                currentPage: 1,
                perPage: 10,
                total: 0,
                items: new List<ConversationQueryModel>()
            ));
        var useCase = new UseCase.ListUserConversations(repositoryMock.Object);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(0);
        output.Items.Should().BeEmpty();

        repositoryMock.Verify(x => x.Search(
            It.Is<IQuerySpecification<ConversationQueryModel>>(spec =>
                spec.Criteria != null &&
                spec.Page == input.Page &&
                spec.PerPage == input.PerPage),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory(DisplayName = nameof(ListInputWithoutAllParameters))]
    [Trait("Chat/Application", "ListUserConversations - Use Cases")]
    [MemberData(
        nameof(ListUserConversationsTestDataGenerator.GetInputsWithoutAllParameter),
        parameters: 14,
        MemberType = typeof(ListUserConversationsTestDataGenerator)
    )]
    public async Task ListInputWithoutAllParameters(ListUserConversationsInput input)
    {
        var userId = input.UserId;
        var repositoryMock = GetConversationReadRepositoryMock();
        var listConversations = CreateExampleConversationsList(userId, 5);
        var expectedItems = listConversations
            .Where(c => c.ParticipantIds.Contains(userId)
                        && (string.IsNullOrWhiteSpace(input.Search)
                            || c.Title.Contains(input.Search, StringComparison.InvariantCultureIgnoreCase)))
            .ToList();
        repositoryMock
            .Setup(x => x.Search(
                It.Is<IQuerySpecification<ConversationQueryModel>>(spec =>
                    spec.Criteria != null &&
                    spec.Page == (input.Page <= 0 ? 1 : input.Page) &&
                    spec.PerPage == (input.PerPage <= 0 ? 20 : Math.Min(input.PerPage, 100))),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchOutput<ConversationQueryModel>(
                currentPage: input.Page <= 0 ? 1 : input.Page,
                perPage: input.PerPage <= 0 ? 20 : Math.Min(input.PerPage, 100),
                total: expectedItems.Count,
                items: expectedItems
            ));
        var useCase = new UseCase.ListUserConversations(
            repositoryMock.Object
        );

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(input.Page <= 0 ? 1 : input.Page);
        output.PerPage.Should().Be(input.PerPage <= 0 ? 20 : Math.Min(input.PerPage, 100));
        output.Total.Should().Be(expectedItems.Count);
        output.Items.Should().HaveCount(expectedItems.Count);
        output.Items.ToList().ForEach(outputItem =>
        {
            var expectedItem = listConversations.FirstOrDefault(c => c.Id == outputItem.Id);
            expectedItem.Should().NotBeNull();
            outputItem.Should().NotBeNull();
            outputItem.Id.Should().Be(expectedItem!.Id);
            outputItem.Title.Should().Be(expectedItem!.Title);
            outputItem.CreatedAt.Should().Be(expectedItem.CreatedAt);
            outputItem.UpdatedAt.Should().Be(expectedItem.UpdatedAt);
            outputItem.ParticipantIds.Should().BeEquivalentTo(expectedItem.ParticipantIds);
            if (expectedItem.LastMessage is not null)
            {
                outputItem.LastMessage.Should().NotBeNull();
                outputItem.LastMessage!.MessageId.Should().Be(expectedItem.LastMessage.MessageId);
                outputItem.LastMessage.Content.Should().Be(expectedItem.LastMessage.Content);
                outputItem.LastMessage.SentAt.Should().Be(expectedItem.LastMessage.SentAt);
                outputItem.LastMessage.Status.Should().Be(expectedItem.LastMessage.Status);
            }
            else
            {
                outputItem.LastMessage.Should().BeNull();
            }
        });

        repositoryMock.Verify(x => x.Search(
            It.Is<IQuerySpecification<ConversationQueryModel>>(spec =>
                spec.Criteria != null &&
                spec.Page == input.Page &&
                spec.PerPage == input.PerPage &&
                expectedItems.All(e => spec.Criteria!.Compile()(e))
            ),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
