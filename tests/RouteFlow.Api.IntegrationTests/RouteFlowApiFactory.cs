using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RouteFlow.Api.IntegrationTests;

public sealed class RouteFlowApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Production);
        builder.UseSetting(
            "ConnectionStrings:deliveries",
            "Host=localhost;Port=1;Database=route_flow_tests;Username=postgres;Password=postgres");
        builder.UseSetting(
            "ConnectionStrings:fleet",
            "Host=localhost;Port=1;Database=route_flow_tests;Username=postgres;Password=postgres");
        builder.ConfigureServices(services =>
        {
            services.AddLogging(logging => logging.ClearProviders());
            services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);
            services.RemoveAll<IHostedService>();
        });
    }
}
