using Helix.Chat.Application.UseCases.Conversation.CreateConversation;

namespace Helix.Chat.EndToEndTests.Api.Conversation.CreateConversation;

[Collection(nameof(CreateConversationApiTestFixture))]
public class CreateConversationApiTest(CreateConversationApiTestFixture fixture)
    : IDisposable
{
    private readonly CreateConversationApiTestFixture _fixture = fixture;

    [Fact(DisplayName = nameof(CreateConversation))]
    [Trait("Chat/EndToEnd/API", "Conversation/Create - Endpoints")]
    public async Task CreateConversation()
    {
        var input = _fixture.GetValidInput();

        var (httpMessage, response) = await _fixture
            .ApiClient.Post<TestApiResponse<CreateConversationOutput>>(
                "/conversations",
                input
            );

        httpMessage.Should().NotBeNull();
        httpMessage.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status201Created);
        response.Should().NotBeNull();
        response.Data.Should().NotBeNull();
        response.Data.Id.Should().NotBeEmpty();
        response.Data.CreatedAt.Should()
            .NotBeSameDateAs(default);

        var dbConversation = await _fixture.ConversationPersistence
            .GetById(response.Data.Id);
        dbConversation.Should().NotBeNull();
        dbConversation.Id.Should().NotBeEmpty();
        dbConversation.Title.Should().Be(input.Title);
        dbConversation.Participants.Should().HaveCount(0);
        dbConversation.CreatedAt.Should()
            .NotBeSameDateAs(default);
    }

    [Theory(DisplayName = nameof(ErrorWhenCantInstantiateAggregate))]
    [Trait("Chat/EndToEnd/API", "Conversation/Create - Endpoints")]
    [MemberData(
        nameof(CreateConversationApiTestDataGenerator.GetInvalidInputs),
        MemberType = typeof(CreateConversationApiTestDataGenerator)
    )]
    public async Task ErrorWhenCantInstantiateAggregate(
        CreateConversationInput input,
        string expectedDetail)
    {
        var (httpMessage, response) = await _fixture
            .ApiClient.Post<ProblemDetails>(
                "/conversations",
                input
            );

        httpMessage.Should().NotBeNull();
        httpMessage!.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        response.Should().NotBeNull();
        response!.Title.Should().Be("One or more validation errors occurred.");
        response.Type.Should().Be("UnprocessableEntity");
        response.Status.Should().Be((int)StatusCodes.Status422UnprocessableEntity);
        response.Detail.Should().Be(expectedDetail);
    }

    public void Dispose()
        => _fixture.CleanPersistence();
}
