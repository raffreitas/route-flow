using Microsoft.EntityFrameworkCore;
using RouteFlow.Api.ExceptionHandling;
using RouteFlow.Api.Modules.Deliveries;
using RouteFlow.Deliveries.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddDeliveriesModule(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<DeliveriesDbContext>();
    if ((await dbContext.Database.GetPendingMigrationsAsync()).Any())
        await dbContext.Database.MigrateAsync();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.MapDeliveriesEndpoints();
app.MapDefaultEndpoints();

await app.RunAsync();

public partial class Program;