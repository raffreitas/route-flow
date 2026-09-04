using System.ComponentModel.DataAnnotations;

namespace RouteFlow.Api.Modules.Fleet;

public sealed record RegisterDriverRequest(
    [Required, StringLength(200)] string Name,
    [Required] string VehicleType);
