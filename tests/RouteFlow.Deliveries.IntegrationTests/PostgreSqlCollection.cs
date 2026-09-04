namespace RouteFlow.Deliveries.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class PostgreSqlCollection : ICollectionFixture<PostgreSqlFixture>
{
    public const string Name = "Deliveries PostgreSQL";
}
