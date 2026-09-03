using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace RouteFlow.ArchitectureTests;

public class DeliveriesArchitectureTests
{
    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            typeof(RouteFlow.SharedKernel.IEntity).Assembly,
            typeof(RouteFlow.Deliveries.Domain.AssemblyReference).Assembly,
            typeof(RouteFlow.Deliveries.Application.AssemblyReference).Assembly,
            typeof(RouteFlow.Deliveries.Infrastructure.AssemblyReference).Assembly,
            typeof(RouteFlow.Deliveries.Contracts.AssemblyReference).Assembly
        )
        .Build();

    [Fact]
    public void DomainLayer_ShouldNotDependOn_ApplicationOrInfrastructureLayers()
    {
        var domainTypes = Types().That().ResideInNamespace("RouteFlow.Deliveries.Domain");
        var applicationTypes = Types().That().ResideInNamespace("RouteFlow.Deliveries.Application");
        var infrastructureTypes = Types().That().ResideInNamespace("RouteFlow.Deliveries.Infrastructure");

        var rule = Types().That().Are(domainTypes)
            .Should().NotDependOnAny(applicationTypes)
            .AndShould().NotDependOnAny(infrastructureTypes);

        rule.Check(Architecture);
    }
}
