using Helix.Chat.Query.Models;
using Helix.Chat.UnitTests.Query.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Helix.Chat.UnitTests.Query.Serialization;
[Collection(nameof(QueryBaseFixture))]
public sealed class ConversationQueryModelSerializationTest(QueryBaseFixture fixture)
{
    private readonly QueryBaseFixture _fixture = fixture;

    [Fact(DisplayName = nameof(SerializeRespectsSnakeCaseForConversationModel))]
    [Trait("Chat/Query", "Serialization - SnakeCase")]
    public void SerializeRespectsSnakeCaseForConversationModel()
    {
        BsonMappingTestHelper.EnsureMappingsRegistered();

        var utcNow = DateTime.UtcNow;
        var conversationModel = new ConversationQueryModel
        {
            Id = _fixture.NewId(),
            Title = _fixture.AnyContent(),
            CreatedAt = utcNow.AddHours(-2),
            UpdatedAt = utcNow,
            ParticipantIds = [_fixture.NewId(), _fixture.NewId()],
            LastMessage = new ConversationQueryModel.MessageSnapshot(
                MessageId: _fixture.NewId(),
                Content: _fixture.AnyContent(),
                SentAt: utcNow.AddMinutes(-1),
                Status: "Sent"
            )
        };

        var bsonDoc = conversationModel.ToBsonDocument();

        bsonDoc.Contains("_id").Should().BeTrue();
        bsonDoc.Contains("title").Should().BeTrue();
        bsonDoc.Contains("created_at").Should().BeTrue();
        bsonDoc.Contains("updated_at").Should().BeTrue();
        bsonDoc.Contains("participant_ids").Should().BeTrue();
        bsonDoc.Contains("last_message").Should().BeTrue();

        var lastMessageDoc = bsonDoc["last_message"].AsBsonDocument;
        lastMessageDoc.Contains("message_id").Should().BeTrue();
        lastMessageDoc.Contains("content").Should().BeTrue();
        lastMessageDoc.Contains("sent_at").Should().BeTrue();
        lastMessageDoc.Contains("status").Should().BeTrue();

        var deserializedModel = BsonSerializer.Deserialize<ConversationQueryModel>(bsonDoc);

        deserializedModel.Id.Should().Be(conversationModel.Id);
        deserializedModel.Title.Should().Be(conversationModel.Title);
        deserializedModel.CreatedAt.Should().BeCloseTo(conversationModel.CreatedAt, TimeSpan.FromSeconds(1));
        deserializedModel.UpdatedAt.Should().BeCloseTo(conversationModel.UpdatedAt, TimeSpan.FromSeconds(1));
        deserializedModel.ParticipantIds.Should().BeEquivalentTo(conversationModel.ParticipantIds);

        deserializedModel.LastMessage.Should().NotBeNull();
        deserializedModel.LastMessage!.MessageId.Should().Be(conversationModel.LastMessage!.MessageId);
        deserializedModel.LastMessage.Content.Should().Be(conversationModel.LastMessage.Content);
        deserializedModel.LastMessage.SentAt.Should().BeCloseTo(conversationModel.LastMessage.SentAt, TimeSpan.FromSeconds(1));
        deserializedModel.LastMessage.Status.Should().Be(conversationModel.LastMessage.Status);
    }
}