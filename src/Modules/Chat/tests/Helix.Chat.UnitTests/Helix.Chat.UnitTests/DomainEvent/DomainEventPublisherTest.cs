using Microsoft.Extensions.DependencyInjection;
using Shared.Application;
using Shared.Domain.SeedWorks;

namespace Helix.Chat.UnitTests.DomainEvent;
public class DomainEventPublisherTest
{
    [Fact(DisplayName = nameof(PublishAsync))]
    [Trait("Chat/Application", "DomainEventPublisher")]
    public async Task PublishAsync()
    {
        var serviceCollection = new ServiceCollection();
        var eventHandlerMock1 = new Mock<IDomainEventHandler<DomainEventToBeHandledFake>>();
        var eventHandlerMock2 = new Mock<IDomainEventHandler<DomainEventToBeHandledFake>>();
        var eventHandlerMock3 = new Mock<IDomainEventHandler<DomainEventToNotBeHandledFake>>();
        serviceCollection.AddSingleton(eventHandlerMock1.Object);
        serviceCollection.AddSingleton(eventHandlerMock2.Object);
        serviceCollection.AddSingleton(eventHandlerMock3.Object);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var domainEventPublisher = new DomainEventPublisher(scopeFactory);
        Event.DomainEvent @event = new DomainEventToBeHandledFake();

        await domainEventPublisher.PublishAsync((dynamic)@event, CancellationToken.None);

        eventHandlerMock1.Verify(x => x.HandleAsync((DomainEventToBeHandledFake)@event, It.IsAny<CancellationToken>()),
            Times.Once);
        eventHandlerMock2.Verify(x => x.HandleAsync((DomainEventToBeHandledFake)@event, It.IsAny<CancellationToken>()),
            Times.Once);
        eventHandlerMock3.Verify(x => x.HandleAsync(
            It.IsAny<DomainEventToNotBeHandledFake>(),
            It.IsAny<CancellationToken>()),
        Times.Never);
    }

    [Fact(DisplayName = nameof(NoActionWhenThereIsNoSubscriber))]
    [Trait("Chat/Application", "DomainEventPublisher")]
    public async Task NoActionWhenThereIsNoSubscriber()
    {
        var serviceCollection = new ServiceCollection();
        var eventHandlerMock1 = new Mock<IDomainEventHandler<DomainEventToNotBeHandledFake>>();
        var eventHandlerMock2 = new Mock<IDomainEventHandler<DomainEventToNotBeHandledFake>>();
        serviceCollection.AddSingleton(eventHandlerMock1.Object);
        serviceCollection.AddSingleton(eventHandlerMock2.Object);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var domainEventPublisher = new DomainEventPublisher(scopeFactory);
        var @event = new DomainEventToBeHandledFake();

        await domainEventPublisher.PublishAsync(@event, CancellationToken.None);

        eventHandlerMock1.Verify(x => x.HandleAsync(
            It.IsAny<DomainEventToNotBeHandledFake>(),
            It.IsAny<CancellationToken>()),
        Times.Never);
        eventHandlerMock2.Verify(x => x.HandleAsync(
            It.IsAny<DomainEventToNotBeHandledFake>(),
            It.IsAny<CancellationToken>()),
        Times.Never);
    }
}
