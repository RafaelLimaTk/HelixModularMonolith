using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Helix.Chat.Infra.Data.EF.Configurations;
internal class ConversationConfiguration
    : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.HasKey(conversation => conversation.Id);
        builder.Property(conversation => conversation.Title)
            .HasMaxLength(Conversation.MAX_LENGTH);
        builder.Ignore(conversation => conversation.Participants);
        builder.Ignore(conversation => conversation.Events);
    }
}
