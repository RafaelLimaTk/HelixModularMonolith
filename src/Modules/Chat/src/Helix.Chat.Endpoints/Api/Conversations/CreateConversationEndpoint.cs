using Helix.Chat.Application.UseCases.Conversation.CreateConversation;

namespace Helix.Chat.Endpoints.Api.Conversations;

public class CreateConversationEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/conversations", async (
            CreateConversationInput input,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var output = await mediator.Send(input, cancellationToken);
            return Results.Created(
                $"/conversations/{output.Id}",
                new ApiResponse<CreateConversationOutput>(output));
        })
        .WithName("CreateConversation")
        .WithTags("Conversations")
        .Produces<ApiResponse<CreateConversationOutput>>(StatusCodes.Status201Created)
        .ProducesValidationProblem();
    }
}
