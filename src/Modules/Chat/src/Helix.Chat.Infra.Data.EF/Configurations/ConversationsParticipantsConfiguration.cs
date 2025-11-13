using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Helix.Chat.Infra.Data.EF.Configurations;
internal class ConversationsParticipantsConfiguration
    : IEntityTypeConfiguration<ConversationsParticipants>
{
    public void Configure(EntityTypeBuilder<ConversationsParticipants> builder)
        => builder.HasKey(relation => new
        {
            relation.ConversationId,
            relation.UserId
        });
}
