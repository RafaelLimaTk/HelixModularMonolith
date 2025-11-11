using Helix.Chat.Application.UseCases.Conversation.CreateConversation;
using Helix.Chat.Domain.Events.Conversation;
using Helix.Chat.Domain.Interfaces;
using Helix.Chat.UnitTests.Application.Conversation.Common;
using Helix.Chat.UnitTests.Extensions.DateTime;
using UseCase = Helix.Chat.Application.UseCases.Conversation.CreateConversation;

namespace Helix.Chat.UnitTests.Application.Conversation.CreateConversation;
public class CreateConversationTest : ConversationUseCasesBaseFixture
{
    [Fact(DisplayName = nameof(CreateConversation))]
    [Trait("Chat/Application", "CreateConversation - Use Cases")]
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

    [Fact(DisplayName = nameof(CreateConversationRaiseConversationCreatedDomainEvent))]
    [Trait("Chat/Application", "CreateConversation - Use Cases")]
    public async Task CreateConversationRaiseConversationCreatedDomainEvent()
    {
        var title = Faker.Lorem.Sentence();
        var repository = new Mock<IConversationRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        DomainEntity.Conversation? captured = null;
        repository.Setup(r => r.Insert(
            It.IsAny<DomainEntity.Conversation>(),
            It.IsAny<CancellationToken>()
        ))
        .Callback<DomainEntity.Conversation, CancellationToken>((c, ct) => captured = c)
        .Returns(Task.CompletedTask);

        var useCase = new UseCase.CreateConversation(repository.Object, unitOfWork.Object);
        var input = new CreateConversationInput(title);

        var before = DateTime.UtcNow.TrimMilliseconds();
        var output = await useCase.Handle(input, CancellationToken.None);
        var after = DateTime.UtcNow.TrimMilliseconds();

        captured.Should().NotBeNull();
        captured!.Events.OfType<ConversationCreated>().Should().HaveCount(1);
        var evt = captured.Events.OfType<ConversationCreated>().First();
        evt.ConversationId.Should().Be(captured.Id);
        evt.Title.Should().Be(title);
        evt.CreatedAt.TrimMilliseconds().Should().BeOnOrAfter(before).And.BeOnOrBefore(after);

        repository.Verify(r => r.Insert(
            It.IsAny<DomainEntity.Conversation>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
        unitOfWork.Verify(u => u.Commit(
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact(DisplayName = nameof(ThrowWhenTitleIsEmpty))]
    [Trait("Chat/Application", "CreateConversation - Use Cases")]
    public async Task ThrowWhenTitleIsEmpty()
    {
        var repository = new Mock<IConversationRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var useCase = new UseCase.CreateConversation(repository.Object, unitOfWork.Object);
        var input = new CreateConversationInput(string.Empty);

        Func<Task> action = async ()
            => await useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<EntityValidationException>()
            .WithMessage("Title should not be null or empty");
        repository.Verify(r => r.Insert(
            It.IsAny<DomainEntity.Conversation>(),
            It.IsAny<CancellationToken>()
        ), Times.Never);
        unitOfWork.Verify(u => u.Commit(
            It.IsAny<CancellationToken>()
        ), Times.Never);
    }

    [Theory(DisplayName = nameof(ThrowWhenTitleIsLessThan3Characters))]
    [Trait("Chat/Application", "CreateConversation - Use Cases")]
    [InlineData("a")]
    [InlineData("ab")]
    public async Task ThrowWhenTitleIsLessThan3Characters(string title)
    {
        var repository = new Mock<IConversationRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var useCase = new UseCase.CreateConversation(repository.Object, unitOfWork.Object);
        var input = new CreateConversationInput(title);

        Func<Task> action = async ()
            => await useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<EntityValidationException>()
            .WithMessage("Title should be at least 3 characters long");
        repository.Verify(r => r.Insert(
            It.IsAny<DomainEntity.Conversation>(),
            It.IsAny<CancellationToken>()
        ), Times.Never);
        unitOfWork.Verify(u => u.Commit(
            It.IsAny<CancellationToken>()
        ), Times.Never);
    }

    [Theory(DisplayName = nameof(ThrowWhenTitleIsGreaterThanMaxCharacters))]
    [Trait("Chat/Application", "CreateConversation - Use Cases")]
    [InlineData(1)]
    [InlineData(10)]
    public async Task ThrowWhenTitleIsGreaterThanMaxCharacters(int excess)
    {
        var repository = new Mock<IConversationRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var useCase = new UseCase.CreateConversation(repository.Object, unitOfWork.Object);
        var maxLength = DomainEntity.Conversation.MAX_LENGTH;
        var invalidTitle = new string('a', maxLength + excess);
        var input = new CreateConversationInput(invalidTitle);

        Func<Task> action = async ()
            => await useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<EntityValidationException>()
            .WithMessage($"Title should be at most {maxLength} characters long");
        repository.Verify(r => r.Insert(
            It.IsAny<DomainEntity.Conversation>(),
            It.IsAny<CancellationToken>()
        ), Times.Never);
        unitOfWork.Verify(u => u.Commit(
            It.IsAny<CancellationToken>()
        ), Times.Never);
    }
}
