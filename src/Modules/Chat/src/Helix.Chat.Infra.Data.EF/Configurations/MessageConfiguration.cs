using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Helix.Chat.Infra.Data.EF.Configurations;
internal class MessageConfiguration
    : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.HasKey(message => message.Id);
        builder.Property(message => message.Content)
            .HasMaxLength(Message.MAX_LENGTH);
        builder.HasOne<Conversation>()
               .WithMany()
               .HasForeignKey(x => x.ConversationId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.ConversationId, x.SentAt });
        builder.HasIndex(x => x.Status);
    }
}
