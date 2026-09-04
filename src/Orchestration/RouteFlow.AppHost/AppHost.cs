var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder
    .AddPostgres("postgres")
    .WithDataVolume()
    .WithHostPort(5432);

var deliveriesDatabase = postgres.AddDatabase("deliveries");
var fleetDatabase = postgres.AddDatabase("fleet");

builder
    .AddProject<Projects.RouteFlow_Api>("api")
    .WithReference(deliveriesDatabase)
    .WithReference(fleetDatabase)
    .WaitFor(deliveriesDatabase)
    .WaitFor(fleetDatabase);

await builder.Build().RunAsync();
