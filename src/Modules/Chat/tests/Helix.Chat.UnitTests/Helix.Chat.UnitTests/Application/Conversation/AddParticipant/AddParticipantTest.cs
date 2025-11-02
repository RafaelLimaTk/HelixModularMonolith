using Helix.Chat.Domain.Events.Conversation;
using UseCase = Helix.Chat.Application.UseCases.Conversation.AddParticipant;

namespace Helix.Chat.UnitTests.Application.Conversation.AddParticipant;

[Collection(nameof(AddParticipantTestFixture))]
public class AddParticipantTest(AddParticipantTestFixture fixture)
{
    private readonly AddParticipantTestFixture _fixture = fixture;

    [Fact(DisplayName = nameof(AddParticipant))]
    [Trait("Chat/Application", "AddParticipant - UseCase")]
    public async Task AddParticipant()
    {
        var conversationRepositoryMock = _fixture.GetConversationRepositoryMock();
        var unitOfWork = _fixture.GetUnitOfWorkMock();
        var userId = Guid.NewGuid();
        var exampleConversation = _fixture.GetConversationExample();
        conversationRepositoryMock.Setup(x => x.Get(
            It.Is<Guid>(id => id == exampleConversation.Id),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(exampleConversation);
        conversationRepositoryMock.Setup(x => x.Update(
            It.IsAny<DomainEntity.Conversation>(),
            It.IsAny<CancellationToken>()
        )).Returns(Task.CompletedTask);
        var useCase = new UseCase.AddParticipant(
            conversationRepositoryMock.Object,
            unitOfWork.Object
        );
        var input = new UseCase.AddParticipantInput(
            exampleConversation.Id,
            userId
        );

        var before = DateTime.UtcNow;
        var output = await useCase.Handle(input, CancellationToken.None);
        var after = DateTime.UtcNow;

        output.Should().NotBeNull();
        output.ConversationId.Should().Be(exampleConversation.Id);
        output.UserId.Should().Be(userId);
        output.Added.Should().BeTrue();
        output.JoinedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        conversationRepositoryMock.Verify(x => x.Update(
            It.Is<DomainEntity.Conversation>(c =>
                c.Id == exampleConversation.Id
                && c.Title == exampleConversation.Title
                && c.CreatedAt == exampleConversation.CreatedAt
                && c.Participants.Any(p => p.UserId == userId)
            ),
            It.IsAny<CancellationToken>()
        ), Times.Once);
        unitOfWork.Verify(u => u.Commit(
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact(DisplayName = nameof(AddParticipantRaiseParticipantAddedDomainEvent))]
    [Trait("Chat/Application", "AddParticipant - UseCase")]
    public async Task AddParticipantRaiseParticipantAddedDomainEvent()
    {
        var conversationRepositoryMock = _fixture.GetConversationRepositoryMock();
        var unitOfWork = _fixture.GetUnitOfWorkMock();
        var userId = Guid.NewGuid();
        var exampleConversation = _fixture.GetConversationExample();
        conversationRepositoryMock.Setup(x => x.Get(
            It.Is<Guid>(id => id == exampleConversation.Id),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(exampleConversation);

        DomainEntity.Conversation? captured = null;
        conversationRepositoryMock.Setup(x => x.Update(
            It.IsAny<DomainEntity.Conversation>(),
            It.IsAny<CancellationToken>()
        ))
        .Callback<DomainEntity.Conversation, CancellationToken>((c, ct) => captured = c)
        .Returns(Task.CompletedTask);

        var useCase = new UseCase.AddParticipant(
            conversationRepositoryMock.Object,
            unitOfWork.Object
        );
        var input = new UseCase.AddParticipantInput(
            exampleConversation.Id,
            userId
        );

        var before = DateTime.UtcNow;
        var output = await useCase.Handle(input, CancellationToken.None);
        var after = DateTime.UtcNow;

        output.Should().NotBeNull();
        output.ConversationId.Should().Be(exampleConversation.Id);
        output.UserId.Should().Be(userId);
        output.Added.Should().BeTrue();
        output.JoinedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);

        captured.Should().NotBeNull();
        captured!.Events.OfType<ParticipantAdded>().Should().HaveCount(1);
        var evt = captured.Events.OfType<ParticipantAdded>().First();
        evt.ConversationId.Should().Be(exampleConversation.Id);
        evt.UserId.Should().Be(userId);
        evt.JoinedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);

        conversationRepositoryMock.Verify(x => x.Update(
            It.IsAny<DomainEntity.Conversation>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
        unitOfWork.Verify(u => u.Commit(
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact(DisplayName = nameof(ThrowIfNotFoundConversation))]
    [Trait("Chat/Application", "AddParticipant - UseCase")]
    public async Task ThrowIfNotFoundConversation()
    {
        var conversationRepositoryMock = _fixture.GetConversationRepositoryMock();
        var unitOfWork = _fixture.GetUnitOfWorkMock();
        var userId = Guid.NewGuid();
        var exampleId = Guid.NewGuid();
        conversationRepositoryMock.Setup(x => x.Get(
            It.Is<Guid>(id => id == exampleId),
            It.IsAny<CancellationToken>()
        )).ThrowsAsync(new NotFoundException($"Conversation '{exampleId}' not found."));
        var useCase = new UseCase.AddParticipant(
            conversationRepositoryMock.Object,
            unitOfWork.Object
        );
        var input = new UseCase.AddParticipantInput(
            exampleId,
            userId
        );

        Func<Task> act = async ()
            => await useCase.Handle(input, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Conversation '{exampleId}' not found.");
        conversationRepositoryMock.Verify(x => x.Get(
            It.Is<Guid>(id => id == exampleId),
            It.IsAny<CancellationToken>()
        ), Times.Once);
        conversationRepositoryMock.Verify(x => x.Update(
            It.IsAny<DomainEntity.Conversation>(),
            It.IsAny<CancellationToken>()
        ), Times.Never);
        unitOfWork.Verify(u => u.Commit(
            It.IsAny<CancellationToken>()
        ), Times.Never);
    }

    [Fact(DisplayName = nameof(DoNotAddWhenParticipantAlreadyExists))]
    [Trait("Chat/Application", "AddParticipant - UseCase")]
    public async Task DoNotAddWhenParticipantAlreadyExists()
    {
        var conversationRepositoryMock = _fixture.GetConversationRepositoryMock();
        var unitOfWork = _fixture.GetUnitOfWorkMock();
        var listParticipants = _fixture.GetParticipantIds();
        var exampleConversation = _fixture.GetConversationExample(userIds: listParticipants);
        var existingParticipant = exampleConversation.Participants.First();
        conversationRepositoryMock.Setup(x => x.Get(
            It.Is<Guid>(id => id == exampleConversation.Id),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(exampleConversation);
        conversationRepositoryMock.Setup(x => x.Update(
            It.IsAny<DomainEntity.Conversation>(),
            It.IsAny<CancellationToken>()
        )).Returns(Task.CompletedTask);
        var useCase = new UseCase.AddParticipant(
            conversationRepositoryMock.Object,
            unitOfWork.Object
        );
        var input = new UseCase.AddParticipantInput(
            exampleConversation.Id,
            existingParticipant.UserId
        );

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.ConversationId.Should().Be(exampleConversation.Id);
        output.UserId.Should().Be(existingParticipant.UserId);
        output.Added.Should().BeFalse();
        output.JoinedAt.Should().Be(existingParticipant.JoinedAt);
        conversationRepositoryMock.Verify(x => x.Update(
            It.Is<DomainEntity.Conversation>(c =>
                c.Id == exampleConversation.Id
                && c.Title == exampleConversation.Title
                && c.CreatedAt == exampleConversation.CreatedAt
                && c.Participants.Count(p => p.UserId == existingParticipant.UserId) == 1
            ),
            It.IsAny<CancellationToken>()
        ), Times.Once);
        unitOfWork.Verify(u => u.Commit(
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact(DisplayName = nameof(ThrowWhenUserIdIsEmpty))]
    [Trait("Chat/Application", "AddParticipant - UseCase")]
    public async Task ThrowWhenUserIdIsEmpty()
    {
        var conversationRepositoryMock = _fixture.GetConversationRepositoryMock();
        var unitOfWork = _fixture.GetUnitOfWorkMock();
        var exampleConversation = _fixture.GetConversationExample();
        conversationRepositoryMock.Setup(x => x.Get(
            It.Is<Guid>(id => id == exampleConversation.Id),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(exampleConversation);
        var useCase = new UseCase.AddParticipant(
            conversationRepositoryMock.Object,
            unitOfWork.Object
        );
        var input = new UseCase.AddParticipantInput(
            exampleConversation.Id,
            Guid.Empty
        );

        Func<Task> act = async ()
            => await useCase.Handle(input, CancellationToken.None);

        await act.Should().ThrowAsync<EntityValidationException>()
            .WithMessage("UserId should not be empty");
        conversationRepositoryMock.Verify(x => x.Get(
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
        conversationRepositoryMock.Verify(x => x.Update(
            It.IsAny<DomainEntity.Conversation>(),
            It.IsAny<CancellationToken>()
        ), Times.Never);
        unitOfWork.Verify(u => u.Commit(
            It.IsAny<CancellationToken>()
        ), Times.Never);
    }
}
