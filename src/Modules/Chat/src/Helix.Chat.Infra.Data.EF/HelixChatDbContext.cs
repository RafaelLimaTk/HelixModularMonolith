using Microsoft.EntityFrameworkCore;

namespace Helix.Chat.Infra.Data.EF;
public class HelixChatDbContext(
    DbContextOptions<HelixChatDbContext> options
    ) : DbContext(options)
{
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();

    public DbSet<ConversationsParticipants> ConversationsParticipants
        => Set<ConversationsParticipants>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("chat");
        builder.ApplyConfigurationsFromAssembly(typeof(HelixChatDbContext).Assembly);
        base.OnModelCreating(builder);
    }
}
