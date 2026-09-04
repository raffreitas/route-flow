var builder = DistributedApplication.CreateBuilder(args);

var deliveriesDatabase = builder
    .AddPostgres("postgres")
    .WithDataVolume()
    .WithHostPort(5432)
    .AddDatabase("deliveries");

builder
    .AddProject<Projects.RouteFlow_Api>("api")
    .WithReference(deliveriesDatabase)
    .WaitFor(deliveriesDatabase);

await builder.Build().RunAsync();