namespace Shared.Query.Interfaces;

public interface IQueryModel<TKey>
{
    TKey Id { get; }
}

public interface IQueryModel : IQueryModel<Guid> { }
