using System.Reflection;
using DataForge.Modules.Identity.Presentation.Controllers;
using DataForge.Modules.Onboarding.Presentation.Controllers;
using DataForge.Modules.Processing.Presentation.Controllers;
using DataForge.Modules.Validation.Presentation.Controllers;

namespace DataForge.Web.Composition;

/// <summary>
/// Module presentation assemblies: their MVC controllers become application parts and their routable
/// Razor components are added to the Blazor router.
/// </summary>
public static class PresentationAssemblies
{
    public static IReadOnlyList<Assembly> All { get; } =
    [
        typeof(AuthController).Assembly,
        typeof(ProjectsController).Assembly,
        typeof(ValidationRunsController).Assembly,
        typeof(JobsController).Assembly,
    ];
}
