using Helix.Chat.Application.UseCases.Conversation.AddParticipant;
using Helix.Chat.Endpoints.ApiModels.Conversation;

namespace Helix.Chat.Endpoints.Api.Conversations;

public class AddParticipantEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/conversations/{id:guid}/participants", async (
            Guid id,
            AddParticipantApiInput apiInput,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var input = apiInput.ToInput(id);
            var output = await mediator.Send(input, cancellationToken);
            return Results.Created(
                $"/conversations/{output.ConversationId}/participants/{output.UserId}",
                new ApiResponse<AddParticipantOutput>(output));
        })
        .WithName("AddParticipant")
        .WithTags("Conversations")
        .Produces<ApiResponse<AddParticipantOutput>>(StatusCodes.Status201Created)
        .ProducesValidationProblem();
    }
}