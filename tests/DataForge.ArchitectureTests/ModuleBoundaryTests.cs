using DataForge.ArchitectureTests.Fixtures;

namespace DataForge.ArchitectureTests;

/// <summary>
/// Modules may only talk to each other through their Contracts project; everything else is private to the module.
/// </summary>
public sealed class ModuleBoundaryTests
{
    public static TheoryData<string> Modules => [.. SolutionAssemblies.Modules];

    [Theory]
    [MemberData(nameof(Modules))]
    public void Module_ReferencesOtherModulesOnlyThroughContracts(string module)
    {
        var violations = SolutionAssemblies.All
            .Where(assembly => assembly.GetName().Name!.StartsWith($"DataForge.Modules.{module}.", StringComparison.Ordinal))
            .SelectMany(assembly => SolutionAssemblies.ReferencedDataForgeAssemblies(assembly)
                .Where(reference => reference.StartsWith("DataForge.Modules.", StringComparison.Ordinal)
                    && !reference.StartsWith($"DataForge.Modules.{module}.", StringComparison.Ordinal)
                    && !reference.EndsWith(".Contracts", StringComparison.Ordinal))
                .Select(reference => $"{assembly.GetName().Name} -> {reference}"))
            .ToList();

        violations.Should().BeEmpty();
    }

    [Fact]
    public void Contracts_DependOnlyOnBuildingBlocksApplication()
    {
        var violations = SolutionAssemblies.Layer("Contracts")
            .SelectMany(assembly => SolutionAssemblies.ReferencedDataForgeAssemblies(assembly)
                .Where(reference => reference.StartsWith("DataForge.Modules.", StringComparison.Ordinal)
                    || reference.EndsWith(".Infrastructure", StringComparison.Ordinal)
                    || reference.EndsWith(".Web", StringComparison.Ordinal))
                .Select(reference => $"{assembly.GetName().Name} -> {reference}"))
            .ToList();

        violations.Should().BeEmpty("contracts are the stable public surface of a module");
    }
}
