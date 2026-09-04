using Microsoft.Extensions.Hosting;
using RouteFlow.Api.ExceptionHandling;
using RouteFlow.Api.Modules.Deliveries;

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
}

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.MapDeliveriesEndpoints();
app.MapDefaultEndpoints();

app.Run();

public partial class Program;
