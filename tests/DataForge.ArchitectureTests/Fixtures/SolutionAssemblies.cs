using System.Reflection;

namespace DataForge.ArchitectureTests.Fixtures;

/// <summary>Loads every DataForge assembly from the test output folder and classifies it by module and layer.</summary>
internal static class SolutionAssemblies
{
    public static readonly string[] Modules = ["Identity", "Onboarding", "Validation", "Processing"];

    public static IReadOnlyList<Assembly> All { get; } = Directory
        .EnumerateFiles(AppContext.BaseDirectory, "DataForge.*.dll")
        .Where(path => !Path.GetFileName(path).Contains("Tests", StringComparison.Ordinal))
        .Select(path => Assembly.LoadFrom(path))
        .ToArray();

    public static IEnumerable<Assembly> Layer(string layer) =>
        All.Where(assembly => assembly.GetName().Name!.EndsWith("." + layer, StringComparison.Ordinal));

    public static Assembly Module(string module, string layer) =>
        All.Single(assembly => assembly.GetName().Name == $"DataForge.Modules.{module}.{layer}");

    public static IEnumerable<string> ReferencedDataForgeAssemblies(Assembly assembly) =>
        assembly.GetReferencedAssemblies()
            .Select(reference => reference.Name!)
            .Where(name => name.StartsWith("DataForge.", StringComparison.Ordinal));

    public static IEnumerable<string> ReferencedAssemblies(Assembly assembly) =>
        assembly.GetReferencedAssemblies().Select(reference => reference.Name!);
}
