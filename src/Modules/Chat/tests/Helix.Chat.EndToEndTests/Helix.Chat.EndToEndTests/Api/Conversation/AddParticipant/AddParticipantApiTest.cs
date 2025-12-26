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
}
