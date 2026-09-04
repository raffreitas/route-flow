var builder = DistributedApplication.CreateBuilder(args);

var deliveriesDatabase = builder
    .AddPostgres("postgres")
    .WithDataVolume()
    .AddDatabase("deliveries");

builder
    .AddProject<Projects.RouteFlow_Api>("api")
    .WithReference(deliveriesDatabase)
    .WaitFor(deliveriesDatabase);

builder.Build().Run();
