using Microsoft.EntityFrameworkCore;
using RouteFlow.Api.ExceptionHandling;
using RouteFlow.Api.Modules.Deliveries;
using RouteFlow.Api.Modules.Fleet;
using RouteFlow.Deliveries.Infrastructure.Persistence;
using RouteFlow.Fleet.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.Services.AddValidation();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddDeliveriesModule(builder.Configuration);
builder.Services.AddFleetModule(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<DeliveriesDbContext>();
    if ((await dbContext.Database.GetPendingMigrationsAsync()).Any())
        await dbContext.Database.MigrateAsync();

    var fleetDbContext = scope.ServiceProvider.GetRequiredService<FleetDbContext>();
    if ((await fleetDbContext.Database.GetPendingMigrationsAsync()).Any())
        await fleetDbContext.Database.MigrateAsync();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.MapDeliveriesEndpoints();
app.MapFleetEndpoints();
app.MapDefaultEndpoints();

await app.RunAsync();

public partial class Program;
