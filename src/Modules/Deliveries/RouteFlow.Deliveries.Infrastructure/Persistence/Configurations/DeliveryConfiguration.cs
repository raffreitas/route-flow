using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RouteFlow.Deliveries.Domain;
using RouteFlow.Deliveries.Domain.Entities;
using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Infrastructure.Persistence.Configurations;

internal sealed class DeliveryConfiguration : IEntityTypeConfiguration<Delivery>
{
    public void Configure(EntityTypeBuilder<Delivery> builder)
    {
        builder.ToTable("deliveries");
        builder.HasKey(delivery => delivery.Id);

        builder.Property(delivery => delivery.Id)
            .HasConversion(id => id.Value, value => DeliveryId.From(value))
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(delivery => delivery.MerchantId)
            .HasConversion(id => id.Value, value => MerchantId.From(value))
            .HasColumnName("merchant_id");

        builder.Property(delivery => delivery.AssignedDriverId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? DriverId.From(value.Value) : null)
            .HasColumnName("assigned_driver_id");

        builder.Property(delivery => delivery.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .HasColumnName("status");

        builder.Property(delivery => delivery.CurrentCustody)
            .HasConversion<string>()
            .HasMaxLength(16)
            .HasColumnName("current_custody");

        builder.Property(delivery => delivery.RequiredVehicleType)
            .HasConversion<string>()
            .HasMaxLength(24)
            .HasColumnName("required_vehicle_type");

        builder.Property(delivery => delivery.CreatedAt).HasColumnName("created_at");
        builder.Property(delivery => delivery.UpdatedAt).HasColumnName("updated_at");
        builder.Property(delivery => delivery.Version)
            .IsConcurrencyToken()
            .HasColumnName("version");

        builder.Ignore(delivery => delivery.DomainEvents);

        ConfigureAddress(builder);
        ConfigurePackage(builder);
        ConfigureAttempts(builder);
    }

    private static void ConfigureAddress(EntityTypeBuilder<Delivery> builder)
    {
        builder.OwnsOne(delivery => delivery.Address, address =>
        {
            address.Property(value => value.Street).HasMaxLength(200).HasColumnName("address_street");
            address.Property(value => value.Number).HasMaxLength(30).HasColumnName("address_number");
            address.Property(value => value.Complement).HasMaxLength(100).HasColumnName("address_complement");
            address.Property(value => value.Neighborhood).HasMaxLength(100).HasColumnName("address_neighborhood");
            address.Property(value => value.City).HasMaxLength(100).HasColumnName("address_city");
            address.Property(value => value.State).HasMaxLength(2).HasColumnName("address_state");
            address.Property(value => value.ZipCode).HasMaxLength(16).HasColumnName("address_zip_code");
            address.Ignore(value => value.Formatted);
        });
    }

    private static void ConfigurePackage(EntityTypeBuilder<Delivery> builder)
    {
        builder.OwnsOne(delivery => delivery.Package, package =>
        {
            package.Property(value => value.WeightKg)
                .HasPrecision(10, 3)
                .HasColumnName("package_weight_kg");
            package.Property(value => value.Description)
                .HasMaxLength(500)
                .HasColumnName("package_description");

            package.OwnsOne(value => value.Dimensions, dimensions =>
            {
                dimensions.Property(value => value.LengthCm)
                    .HasPrecision(10, 2)
                    .HasColumnName("package_length_cm");
                dimensions.Property(value => value.WidthCm)
                    .HasPrecision(10, 2)
                    .HasColumnName("package_width_cm");
                dimensions.Property(value => value.HeightCm)
                    .HasPrecision(10, 2)
                    .HasColumnName("package_height_cm");
                dimensions.Ignore(value => value.VolumeM3);
            });
        });
    }

    private static void ConfigureAttempts(EntityTypeBuilder<Delivery> builder)
    {
        builder.HasMany(delivery => delivery.Attempts)
            .WithOne()
            .HasForeignKey("DeliveryId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(delivery => delivery.Attempts).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
