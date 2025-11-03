using MongoDB.Driver;

namespace Shared.Query.Interfaces;
public interface IReadDbContext
{
    IMongoCollection<T> GetCollection<T>(string? overrideName = null);
}