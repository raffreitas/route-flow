using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RouteFlow.Api.IntegrationTests;

public sealed class CreateDeliveryEndpointTests(RouteFlowApiFactory factory)
    : IClassFixture<RouteFlowApiFactory>
{
    [Fact]
    public async Task CreateDelivery_EmptyMerchantIdString_ShouldReturnBadRequest()
    {
        using var client = factory.CreateClient();
        var request = new
        {
            merchantId = "",
            address = new
            {
                street = "Av. Paulista",
                number = "1000",
                neighborhood = "Bela Vista",
                city = "São Paulo",
                state = "SP",
                zipCode = "01310-100"
            },
            package = new
            {
                weightKg = 1,
                lengthCm = 10,
                widthCm = 20,
                heightCm = 30,
                description = "Envelope"
            }
        };

        var response = await client.PostAsJsonAsync("/deliveries", request);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.Equal("Invalid request", problem.Title);
    }
}
