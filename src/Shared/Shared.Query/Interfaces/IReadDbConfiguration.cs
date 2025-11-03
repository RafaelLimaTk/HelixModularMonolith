using MongoDB.Bson;
using MongoDB.Driver;

namespace Shared.Query.Interfaces;
public interface IReadDbConfiguration
{
    string CollectionName { get; }
    Type ModelType { get; }
    void ConfigureClassMap();
    void ConfigureIndexes(IMongoCollection<BsonDocument> rawCollection);
}
