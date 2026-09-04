using Dapper;
using Microsoft.EntityFrameworkCore;
using RouteFlow.Deliveries.Application.Abstractions;
using RouteFlow.Deliveries.Application.Deliveries.GetDelivery;
using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Infrastructure.Persistence.Queries;

internal sealed class DeliveryQueries(DeliveriesDbContext dbContext) : IDeliveryQueries
{
    private const string GetByIdSql =
        """
        SELECT
            id AS "Id",
            merchant_id AS "MerchantId",
            status AS "Status",
            current_custody AS "Custody",
            assigned_driver_id AS "AssignedDriverId",
            required_vehicle_type AS "RequiredVehicleType",
            address_street AS "AddressStreet",
            address_number AS "AddressNumber",
            address_complement AS "AddressComplement",
            address_neighborhood AS "AddressNeighborhood",
            address_city AS "AddressCity",
            address_state AS "AddressState",
            address_zip_code AS "AddressZipCode",
            package_weight_kg AS "PackageWeightKg",
            package_length_cm AS "PackageLengthCm",
            package_width_cm AS "PackageWidthCm",
            package_height_cm AS "PackageHeightCm",
            package_description AS "PackageDescription",
            created_at AS "CreatedAt",
            updated_at AS "UpdatedAt",
            version AS "Version"
        FROM deliveries.deliveries
        WHERE id = @DeliveryId;

        SELECT
            attempt_number AS "AttemptNumber",
            failure_category AS "FailureCategory",
            failure_description AS "FailureDescription",
            occurred_at AS "OccurredAt",
            notes AS "Notes"
        FROM deliveries.delivery_attempts
        WHERE delivery_id = @DeliveryId
        ORDER BY attempt_number;
        """;

    public async Task<DeliveryDetails?> GetByIdAsync(
        DeliveryId deliveryId,
        CancellationToken cancellationToken = default)
    {
        var connection = dbContext.Database.GetDbConnection();
        var command = new CommandDefinition(
            GetByIdSql,
            new { DeliveryId = deliveryId.Value },
            cancellationToken: cancellationToken);
        using var result = await connection.QueryMultipleAsync(command);
        var delivery = await result.ReadSingleOrDefaultAsync<DeliveryQueryRow>();
        var attempts = (await result.ReadAsync<DeliveryAttemptQueryRow>()).AsList();

        if (delivery is null)
        {
            return null;
        }

        return new DeliveryDetails(
            delivery.Id,
            delivery.MerchantId,
            delivery.Status,
            delivery.Custody,
            delivery.AssignedDriverId,
            delivery.RequiredVehicleType,
            new DeliveryAddressDetails(
                delivery.AddressStreet,
                delivery.AddressNumber,
                delivery.AddressComplement,
                delivery.AddressNeighborhood,
                delivery.AddressCity,
                delivery.AddressState,
                delivery.AddressZipCode),
            new PackageDetails(
                delivery.PackageWeightKg,
                delivery.PackageLengthCm,
                delivery.PackageWidthCm,
                delivery.PackageHeightCm,
                delivery.PackageDescription),
            delivery.CreatedAt,
            delivery.UpdatedAt,
            checked((uint)delivery.Version),
            attempts.Select(attempt => new DeliveryAttemptDetails(
                    attempt.AttemptNumber,
                    attempt.FailureCategory,
                    attempt.FailureDescription,
                    attempt.OccurredAt,
                    attempt.Notes))
                .ToArray());
    }
}
