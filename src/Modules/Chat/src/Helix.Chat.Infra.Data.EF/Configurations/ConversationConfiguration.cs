using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Helix.Chat.Infra.Data.EF.Configurations;

internal class ConversationConfiguration
    : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.HasKey(conversation => conversation.Id);

        builder.Navigation(conversation => conversation.Participants).AutoInclude();

        builder.Property(conversation => conversation.Title)
            .HasMaxLength(Conversation.MAX_LENGTH);

        builder.OwnsMany<Participant>(nameof(Conversation.Participants),
        navigation =>
        {
            navigation.ToTable("conversations_participants");
            navigation.WithOwner()
                .HasForeignKey("ConversationId");
            navigation.Property<Guid>("Id");
            navigation.HasKey("Id");
            navigation.Property(p => p.UserId)
                .IsRequired();
            navigation.Property(p => p.JoinedAt)
                .IsRequired();
        });
        var participantsNavigation = builder.Metadata
            .FindNavigation(nameof(Conversation.Participants))!;
        participantsNavigation.SetPropertyAccessMode(PropertyAccessMode.Field);
        participantsNavigation.SetField("_participants");

        builder.Ignore(conversation => conversation.Events);
    }
}
