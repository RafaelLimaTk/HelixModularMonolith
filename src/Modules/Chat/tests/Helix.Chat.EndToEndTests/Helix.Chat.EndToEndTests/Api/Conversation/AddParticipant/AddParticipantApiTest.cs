using Helix.Chat.Application.UseCases.Conversation.AddParticipant;

namespace Helix.Chat.EndToEndTests.Api.Conversation.AddParticipant;

[Collection(nameof(AddParticipantApiTestFixture))]
public class AddParticipantApiTest(AddParticipantApiTestFixture fixture)
{
    private readonly AddParticipantApiTestFixture _fixture = fixture;

    [Fact(DisplayName = nameof(AddParticipant))]
    [Trait("Chat/EndToEnd/API", "Conversation/AddParticipant - Endpoints")]
    public async Task AddParticipant()
    {
        var conversationsList = _fixture.GetExampleConversationsList(10);
        await _fixture.ConversationPersistence.InsertList(conversationsList);
        var exampleConversation = conversationsList[0];
        var participantId = Guid.NewGuid();
        var input = _fixture.GetValidInput(participantId);

        var beforeAdd = DateTime.UtcNow;
        var (httpMessage, response) = await _fixture
            .ApiClient.Post<TestApiResponse<AddParticipantOutput>>(
                $"/conversations/{exampleConversation.Id}/participants",
                input
            );
        var afterAdd = DateTime.UtcNow;

        httpMessage.Should().NotBeNull();
        httpMessage.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status201Created);
        response.Should().NotBeNull();
        response.Data.Should().NotBeNull();
        response.Data.ConversationId.Should().Be(exampleConversation.Id);
        response.Data.UserId.Should().Be(participantId);
        response.Data.JoinedAt.Should()
            .BeOnOrAfter(beforeAdd)
            .And.BeOnOrBefore(afterAdd);
        response.Data.Added.Should().BeTrue();
        var dbConversation = await _fixture.ConversationPersistence
            .GetById(exampleConversation.Id);
        dbConversation.Should().NotBeNull();
        dbConversation.Id.Should().Be(exampleConversation.Id);
        dbConversation!.Participants.Should().ContainSingle(p => p.UserId == participantId);
    }

    [Fact(DisplayName = nameof(ErrorWhenConversationNotFound))]
    [Trait("Chat/EndToEnd/API", "Conversation/AddParticipant - Endpoints")]
    public async Task ErrorWhenConversationNotFound()
    {
        var conversationsList = _fixture.GetExampleConversationsList(5);
        await _fixture.ConversationPersistence.InsertList(conversationsList);
        var participantId = Guid.NewGuid();
        var input = _fixture.GetValidInput(participantId);
        var randomGuid = Guid.NewGuid();

        var (httpMessage, response) = await _fixture
            .ApiClient.Post<ProblemDetails>(
                $"/conversations/{randomGuid}/participants",
                input
            );

        httpMessage.Should().NotBeNull();
        httpMessage.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status404NotFound);
        response.Should().NotBeNull();
        response.Title.Should().Be("Not Found");
        response.Type.Should().Be("NotFound");
        response.Status.Should().Be(StatusCodes.Status404NotFound);
        response.Detail.Should().Be($"Conversation '{randomGuid}' not found.");
    }

    [Fact(DisplayName = nameof(NotAddExistingParticipant))]
    [Trait("Chat/EndToEnd/API", "Conversation/AddParticipant - Endpoints")]
    public async Task NotAddExistingParticipant()
    {
        var conversationsList = _fixture.GetExampleConversationsList(10);
        var existingParticipants =
            Enumerable.Range(0, conversationsList.Count)
            .Select(_ => Guid.NewGuid())
            .ToList();
        conversationsList.ForEach(conversation =>
        {
            var participantId = existingParticipants[conversationsList.IndexOf(conversation)];
            conversation.AddParticipant(participantId);
        });
        await _fixture.ConversationPersistence.InsertList(conversationsList);
        var exampleConversation = conversationsList[0];
        var existingParticipant = exampleConversation.Participants.First();
        var input = _fixture.GetValidInput(existingParticipant.UserId);

        var (httpMessage, response) = await _fixture
            .ApiClient.Post<TestApiResponse<AddParticipantOutput>>(
                $"/conversations/{exampleConversation.Id}/participants",
                input
            );

        httpMessage.Should().NotBeNull();
        httpMessage.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status201Created);
        response.Should().NotBeNull();
        response.Data.Should().NotBeNull();
        response.Data.ConversationId.Should().Be(exampleConversation.Id);
        response.Data.UserId.Should().Be(existingParticipant.UserId);
        response.Data.Added.Should().BeFalse();
        var dbConversation = await _fixture.ConversationPersistence
            .GetById(exampleConversation.Id);
        dbConversation.Should().NotBeNull();
        dbConversation.Id.Should().Be(exampleConversation.Id);
        dbConversation!.Participants.Should().ContainSingle(p => p.UserId == existingParticipant.UserId);
    }
}
