using DataForge.BuildingBlocks.Domain.Results;

namespace DataForge.Modules.Onboarding.Domain.Errors;

public static class ProjectErrors
{
    public static readonly Error NotFound = Error.NotFound("project.not_found", "The project was not found.");

    public static readonly Error NameAlreadyExists = Error.Conflict("project.name_exists", "A project with this name already exists.");
}
