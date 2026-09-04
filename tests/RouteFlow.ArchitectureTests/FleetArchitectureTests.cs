using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace RouteFlow.ArchitectureTests;

public sealed class FleetArchitectureTests
{
    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            typeof(RouteFlow.Fleet.Domain.AssemblyReference).Assembly,
            typeof(RouteFlow.Fleet.Application.AssemblyReference).Assembly,
            typeof(RouteFlow.Fleet.Infrastructure.AssemblyReference).Assembly,
            typeof(RouteFlow.Fleet.Contracts.AssemblyReference).Assembly)
        .Build();

    [Fact]
    public void DomainLayer_ShouldNotDependOn_ApplicationOrInfrastructureLayers()
    {
        var domainTypes = Types().That().ResideInNamespace("RouteFlow.Fleet.Domain");
        var applicationTypes = Types().That().ResideInNamespace("RouteFlow.Fleet.Application");
        var infrastructureTypes = Types().That().ResideInNamespace("RouteFlow.Fleet.Infrastructure");

        Types().That().Are(domainTypes)
            .Should().NotDependOnAny(applicationTypes)
            .AndShould().NotDependOnAny(infrastructureTypes)
            .Check(Architecture);
    }

    [Fact]
    public void ApplicationLayer_ShouldNotDependOn_InfrastructureLayer()
    {
        var applicationTypes = Types().That().ResideInNamespace("RouteFlow.Fleet.Application");
        var infrastructureTypes = Types().That().ResideInNamespace("RouteFlow.Fleet.Infrastructure");

        Types().That().Are(applicationTypes)
            .Should().NotDependOnAny(infrastructureTypes)
            .Check(Architecture);
    }

    [Fact]
    public void ContractsLayer_ShouldNotDependOn_InternalModuleLayers()
    {
        var contractTypes = Types().That().ResideInNamespace("RouteFlow.Fleet.Contracts");
        var domainTypes = Types().That().ResideInNamespace("RouteFlow.Fleet.Domain");
        var applicationTypes = Types().That().ResideInNamespace("RouteFlow.Fleet.Application");
        var infrastructureTypes = Types().That().ResideInNamespace("RouteFlow.Fleet.Infrastructure");

        Types().That().Are(contractTypes)
            .Should().NotDependOnAny(domainTypes)
            .AndShould().NotDependOnAny(applicationTypes)
            .AndShould().NotDependOnAny(infrastructureTypes)
            .Check(Architecture);
    }
}
