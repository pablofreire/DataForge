using DataForge.ArchitectureTests.Fixtures;

namespace DataForge.ArchitectureTests;

/// <summary>Clean Architecture dependency direction inside each module: Domain &lt;- Application &lt;- Infrastructure / Presentation.</summary>
public sealed class LayerDependencyTests
{
    [Fact]
    public void Domain_DependsOnNoOtherLayerAndNoFramework()
    {
        var violations = SolutionAssemblies.Layer("Domain")
            .SelectMany(assembly => SolutionAssemblies.ReferencedAssemblies(assembly)
                .Where(reference => reference.EndsWith(".Application", StringComparison.Ordinal)
                    || reference.EndsWith(".Infrastructure", StringComparison.Ordinal)
                    || reference.EndsWith(".Presentation", StringComparison.Ordinal)
                    || reference is "Dapper" or "Npgsql" or "FluentValidation"
                    || reference.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal))
                .Select(reference => $"{assembly.GetName().Name} -> {reference}"))
            .ToList();

        violations.Should().BeEmpty();
    }

    [Fact]
    public void Application_DoesNotDependOnInfrastructurePresentationOrDataAccess()
    {
        var violations = SolutionAssemblies.Layer("Application")
            .SelectMany(assembly => SolutionAssemblies.ReferencedAssemblies(assembly)
                .Where(reference => reference.EndsWith(".Infrastructure", StringComparison.Ordinal)
                    || reference.EndsWith(".Presentation", StringComparison.Ordinal)
                    || reference.EndsWith(".Web", StringComparison.Ordinal)
                    || reference is "Dapper" or "Npgsql"
                    || reference.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal))
                .Select(reference => $"{assembly.GetName().Name} -> {reference}"))
            .ToList();

        violations.Should().BeEmpty();
    }

    [Fact]
    public void Presentation_DoesNotDependOnInfrastructureOrDataAccess()
    {
        var violations = SolutionAssemblies.Layer("Presentation")
            .SelectMany(assembly => SolutionAssemblies.ReferencedAssemblies(assembly)
                .Where(reference => reference.EndsWith(".Infrastructure", StringComparison.Ordinal)
                    || reference is "Dapper" or "Npgsql")
                .Select(reference => $"{assembly.GetName().Name} -> {reference}"))
            .ToList();

        violations.Should().BeEmpty("pages and controllers only call use cases");
    }

    [Fact]
    public void Presentation_DoesNotUseDomainTypesDirectly()
    {
        var violations = SolutionAssemblies.Layer("Presentation")
            .SelectMany(assembly => NetArchTest.Rules.Types.InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAny(SolutionAssemblies.Modules.Select(module => $"DataForge.Modules.{module}.Domain").ToArray())
                .GetResult()
                .FailingTypeNames ?? [])
            .ToList();

        violations.Should().BeEmpty("the presentation layer works with request/response types, never with entities");
    }
}
