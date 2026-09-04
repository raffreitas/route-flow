using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RouteFlow.Deliveries.Domain.Entities;
using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Infrastructure.Persistence.Configurations;

internal sealed class DeliveryAttemptConfiguration : IEntityTypeConfiguration<DeliveryAttempt>
{
    public void Configure(EntityTypeBuilder<DeliveryAttempt> builder)
    {
        builder.ToTable("delivery_attempts");
        builder.HasKey(attempt => attempt.Id);

        builder.Property(attempt => attempt.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");
        builder.Property(attempt => attempt.AttemptNumber).HasColumnName("attempt_number");
        builder.Property(attempt => attempt.OccurredAt).HasColumnName("occurred_at");
        builder.Property(attempt => attempt.Notes).HasMaxLength(1000).HasColumnName("notes");
        builder.Property<DeliveryId>("DeliveryId")
            .HasConversion(id => id.Value, value => DeliveryId.From(value))
            .HasColumnName("delivery_id");
        builder.HasIndex("DeliveryId", nameof(DeliveryAttempt.AttemptNumber)).IsUnique();

        builder.OwnsOne(attempt => attempt.Reason, reason =>
        {
            reason.Property(value => value.Category)
                .HasConversion<string>()
                .HasMaxLength(32)
                .HasColumnName("failure_category");
            reason.Property(value => value.Description)
                .HasMaxLength(500)
                .HasColumnName("failure_description");
            reason.Ignore(value => value.AllowsImmediateRetry);
            reason.Ignore(value => value.RequiresOperationalIssueQueue);
        });
    }
}
