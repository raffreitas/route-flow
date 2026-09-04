using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RouteFlow.Deliveries.Infrastructure.Persistence.Outbox;

namespace RouteFlow.Deliveries.Infrastructure.Persistence.Configurations;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(message => message.Id);

        builder.Property(message => message.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");
        builder.Property(message => message.AggregateId).HasColumnName("aggregate_id");
        builder.Property(message => message.OccurredAt).HasColumnName("occurred_at");
        builder.Property(message => message.Type)
            .HasMaxLength(500)
            .HasColumnName("type");
        builder.Property(message => message.Content)
            .HasColumnType("jsonb")
            .HasColumnName("content");
        builder.Property(message => message.ProcessedAt).HasColumnName("processed_at");
        builder.Property(message => message.LastAttemptAt).HasColumnName("last_attempt_at");
        builder.Property(message => message.AttemptCount).HasColumnName("attempt_count");
        builder.Property(message => message.Error)
            .HasMaxLength(2000)
            .HasColumnName("error");
        builder.Property(message => message.TraceParent)
            .HasMaxLength(55)
            .HasColumnName("trace_parent");
        builder.Property(message => message.TraceState)
            .HasMaxLength(512)
            .HasColumnName("trace_state");

        builder.HasIndex(message => new { message.ProcessedAt, message.OccurredAt });
        builder.HasIndex(message => message.AggregateId);
    }
}
