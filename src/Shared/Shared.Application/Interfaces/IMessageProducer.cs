namespace Shared.Application.Interfaces;
public interface IMessageProducer
{
    Task SendMessageAsync<T>(T message, CancellationToken cancellationToken);
}