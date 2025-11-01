namespace Helix.Chat.UnitTests.DomainEvent;

/// <summary>
/// Evento de domínio falso usado nos testes para representar um evento que deve ser tratado pelo manipulador de eventos.
/// </summary>
public class DomainEventToBeHandledFake : Event.DomainEvent
{
}

/// <summary>
/// Evento de domínio falso usado nos testes para representar um evento que NÃO deve ser tratado pelo manipulador de eventos.
/// </summary>
public class DomainEventToNotBeHandledFake : Event.DomainEvent
{
}
