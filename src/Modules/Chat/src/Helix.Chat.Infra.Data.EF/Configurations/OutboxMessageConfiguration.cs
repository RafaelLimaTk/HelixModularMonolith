using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Helix.Chat.Infra.Data.EF.Configurations;
internal class OutboxMessageConfiguration
    : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(outbox => outbox.Id);
        builder.Property(outbox => outbox.Type)
            .HasMaxLength(512);
        builder.HasIndex(outbox => outbox.ProcessedOnUtc);
        builder.HasIndex(outbox => new { outbox.Type, outbox.ProcessedOnUtc });
    }
}
