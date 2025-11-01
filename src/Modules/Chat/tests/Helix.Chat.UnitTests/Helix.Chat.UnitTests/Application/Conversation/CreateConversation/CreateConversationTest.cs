using Helix.Chat.Application.UseCases.Conversation.CreateConversation;
using Helix.Chat.Domain.Interfaces;
using Helix.Chat.UnitTests.Extensions.DateTime;
using UseCase = Helix.Chat.Application.UseCases.Conversation.CreateConversation;

namespace Helix.Chat.UnitTests.Application.Conversation.CreateConversation;
public class CreateConversationTest : BaseFixture
{
    [Fact(DisplayName = nameof(CreateConversation))]
    [Trait("Chat/Application", "CreateConversation - UseCase")]
    public async Task CreateConversation()
    {
        var title = Faker.Lorem.Sentence();
        var repository = new Mock<IConversationRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        repository.Setup(r => r.Insert(
            It.IsAny<DomainEntity.Conversation>(),
            It.IsAny<CancellationToken>()
        )).Returns(Task.CompletedTask);
        var useCase = new UseCase.CreateConversation(repository.Object, unitOfWork.Object);
        var input = new CreateConversationInput(title);

        var before = DateTime.UtcNow.TrimMilliseconds();
        var output = await useCase.Handle(input, CancellationToken.None);
        var after = DateTime.UtcNow.TrimMilliseconds();

        output.Should().NotBeNull();
        output.Id.Should().NotBeEmpty();
        output.CreatedAt.TrimMilliseconds()
            .Should().BeOnOrAfter(before).And.BeOnOrBefore(after);

        repository.Verify(r => r.Insert(
            It.Is<DomainEntity.Conversation>(c =>
                c.Id == output.Id
                && c.Title == title
                && c.CreatedAt.TrimMilliseconds() >= before
                && c.CreatedAt.TrimMilliseconds() <= after
            ),
            It.IsAny<CancellationToken>()
        ), Times.Once);
        unitOfWork.Verify(u => u.Commit(
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}
