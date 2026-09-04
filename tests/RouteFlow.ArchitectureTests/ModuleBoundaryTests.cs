using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace RouteFlow.ArchitectureTests;

public sealed class ModuleBoundaryTests
{
    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            typeof(RouteFlow.Deliveries.Domain.AssemblyReference).Assembly,
            typeof(RouteFlow.Deliveries.Application.AssemblyReference).Assembly,
            typeof(RouteFlow.Deliveries.Infrastructure.AssemblyReference).Assembly,
            typeof(RouteFlow.Deliveries.Contracts.AssemblyReference).Assembly,
            typeof(RouteFlow.Fleet.Domain.AssemblyReference).Assembly,
            typeof(RouteFlow.Fleet.Application.AssemblyReference).Assembly,
            typeof(RouteFlow.Fleet.Infrastructure.AssemblyReference).Assembly,
            typeof(RouteFlow.Fleet.Contracts.AssemblyReference).Assembly)
        .Build();

    [Fact]
    public void DeliveriesModule_ShouldDependOnlyOn_FleetContracts()
    {
        var deliveriesTypes = Types().That().ResideInNamespace("RouteFlow.Deliveries");
        var fleetDomainTypes = Types().That().ResideInNamespace("RouteFlow.Fleet.Domain");
        var fleetApplicationTypes = Types().That().ResideInNamespace("RouteFlow.Fleet.Application");
        var fleetInfrastructureTypes = Types().That().ResideInNamespace("RouteFlow.Fleet.Infrastructure");

        var rule = Types().That().Are(deliveriesTypes)
            .Should().NotDependOnAny(fleetDomainTypes)
            .AndShould().NotDependOnAny(fleetApplicationTypes)
            .AndShould().NotDependOnAny(fleetInfrastructureTypes)
            .WithoutRequiringPositiveResults();

        rule.Check(Architecture);
    }

    [Fact]
    public void FleetModule_ShouldNotDependOn_DeliveriesModule()
    {
        var fleetTypes = Types().That().ResideInNamespace("RouteFlow.Fleet");
        var deliveriesTypes = Types().That().ResideInNamespace("RouteFlow.Deliveries");

        Types().That().Are(fleetTypes)
            .Should().NotDependOnAny(deliveriesTypes)
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }
}
