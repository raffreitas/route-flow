using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RouteFlow.Fleet.Domain;
using RouteFlow.Fleet.Domain.ValueObjects;

namespace RouteFlow.Fleet.Infrastructure.Persistence.Configurations;

internal sealed class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> builder)
    {
        builder.ToTable("drivers");
        builder.HasKey(driver => driver.Id);
        builder.Property(driver => driver.Id)
            .HasConversion(id => id.Value, value => DriverId.From(value))
            .ValueGeneratedNever()
            .HasColumnName("id");
        builder.Property(driver => driver.Name).HasMaxLength(200).HasColumnName("name");
        builder.Property(driver => driver.VehicleType)
            .HasConversion<string>()
            .HasMaxLength(24)
            .HasColumnName("vehicle_type");
        builder.Property(driver => driver.IsAvailable).HasColumnName("is_available");
        builder.Property(driver => driver.RegisteredAt).HasColumnName("registered_at");
        builder.Property(driver => driver.UpdatedAt).HasColumnName("updated_at");
        builder.Property(driver => driver.Version).IsConcurrencyToken().HasColumnName("version");
        builder.Ignore(driver => driver.DomainEvents);
    }
}
