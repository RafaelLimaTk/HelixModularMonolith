using Helix.Chat.Query.Models;
using Helix.Chat.UnitTests.Query.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Helix.Chat.UnitTests.Query.Serialization;

[Collection(nameof(QueryBaseFixture))]
public sealed class MessageQueryModelSerializationTest(QueryBaseFixture fixture)
{
    private readonly QueryBaseFixture _fixture = fixture;

    [Fact(DisplayName = nameof(SerializeRespectsSnakeCaseForMessageModel))]
    [Trait("Chat/Query", "Serialization - SnakeCase")]
    public void SerializeRespectsSnakeCaseForMessageModel()
    {
        BsonMappingTestHelper.EnsureMappingsRegistered();

        var message = new MessageQueryModel
        {
            Id = _fixture.NewId(),
            ConversationId = _fixture.NewId(),
            SenderId = _fixture.NewId(),
            Content = _fixture.AnyContent(),
            SentAt = DateTime.UtcNow,
            DeliveredAt = null,
            ReadAt = null,
            Status = "Sent"
        };

        var documentMessage = message.ToBsonDocument();

        documentMessage.Contains("_id").Should().BeTrue();
        documentMessage.Contains("conversation_id").Should().BeTrue();
        documentMessage.Contains("sender_id").Should().BeTrue();
        documentMessage.Contains("content").Should().BeTrue();
        documentMessage.Contains("sent_at").Should().BeTrue();
        documentMessage.Contains("delivered_at").Should().BeTrue();
        documentMessage.Contains("read_at").Should().BeTrue();
        documentMessage.Contains("status").Should().BeTrue();
        documentMessage["status"].AsString.Should().Be("Sent");
        var back = BsonSerializer.Deserialize<MessageQueryModel>(documentMessage);
        back.Id.Should().Be(message.Id);
        back.ConversationId.Should().Be(message.ConversationId);
        back.SenderId.Should().Be(message.SenderId);
        back.Content.Should().Be(message.Content);
        back.SentAt.Should().BeCloseTo(message.SentAt, TimeSpan.FromSeconds(1));
        back.DeliveredAt.Should().BeNull();
        back.ReadAt.Should().BeNull();
        back.Status.Should().Be(message.Status);
    }
}
