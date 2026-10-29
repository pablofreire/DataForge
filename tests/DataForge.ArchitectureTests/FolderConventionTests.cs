using DataForge.ArchitectureTests.Fixtures;
using DataForge.BuildingBlocks.Application.UseCases.Interfaces;
using DataForge.BuildingBlocks.Domain.Entities;
using FluentValidation;
using NetArchTest.Rules;

namespace DataForge.ArchitectureTests;

/// <summary>
/// Folder layout documented in docs/engineering/coding-standards.md: one kind of artifact per folder, and an
/// interface never shares a folder with a class. Namespaces mirror folders, so the rules check namespaces.
/// </summary>
public sealed class FolderConventionTests
{
    private static readonly Types AllTypes = Types.InAssemblies(SolutionAssemblies.All);

    [Fact]
    public void Interfaces_LiveInInterfacesFolders() =>
        AssertRule(AllTypes.That().AreInterfaces(),
            rule => rule.ResideInNamespaceMatching(@"\.Interfaces(\.\w+)*$"));

    [Fact]
    public void UseCaseImplementations_LiveInImplementationsFolders() =>
        AssertRule(AllTypes.That().ImplementInterface(typeof(IUseCase<,>)).And().AreClasses(),
            rule => rule.ResideInNamespaceEndingWith(".Implementations"));

    [Fact]
    public void UseCaseRequestsAndResponses_LiveInContractsFolders() =>
        AssertRule(AllTypes.That().ResideInNamespaceContaining(".Application.UseCases.")
            .And().HaveNameMatching("(Request|Response)$"),
            rule => rule.ResideInNamespaceEndingWith(".Contracts"));

    [Fact]
    public void RequestValidators_LiveInValidatorsFolders() =>
        AssertRule(AllTypes.That().Inherit(typeof(AbstractValidator<>)),
            rule => rule.ResideInNamespaceEndingWith(".Validators"));

    [Fact]
    public void Entities_LiveInEntitiesFolders() =>
        AssertRule(AllTypes.That().Inherit(typeof(Entity)).And().AreNotAbstract(),
            rule => rule.ResideInNamespaceEndingWith(".Domain.Entities"));

    [Fact]
    public void DomainEnums_LiveInEnumsFolders() =>
        AssertRule(AllTypes.That().ResideInNamespaceStartingWith("DataForge.Modules.")
            .And().ResideInNamespaceContaining(".Domain").And().Inherit(typeof(Enum)),
            rule => rule.ResideInNamespaceEndingWith(".Domain.Enums"));

    [Fact]
    public void ErrorCatalogs_LiveInErrorsFolders() =>
        AssertRule(AllTypes.That().HaveNameEndingWith("Errors").And().AreClasses(),
            rule => rule.ResideInNamespaceEndingWith(".Domain.Errors"));

    [Fact]
    public void Options_LiveInOptionsFolders() =>
        AssertRule(AllTypes.That().HaveNameEndingWith("Options").And().AreClasses(),
            rule => rule.ResideInNamespaceEndingWith(".Options"));

    [Fact]
    public void Exceptions_LiveInExceptionsFolders() =>
        AssertRule(AllTypes.That().Inherit(typeof(Exception)),
            rule => rule.ResideInNamespaceEndingWith(".Exceptions"));

    [Fact]
    public void ModuleDtos_EndWithDtoAndLiveInDtosFolders()
    {
        AssertRule(AllTypes.That().HaveNameEndingWith("Dto"),
            rule => rule.ResideInNamespaceEndingWith(".Contracts.Dtos"));
        AssertRule(AllTypes.That().ResideInNamespaceEndingWith(".Contracts.Dtos"),
            rule => rule.HaveNameEndingWith("Dto"));
    }

    /// <summary>Fails when the selection is empty, so a rule can never pass vacuously after a rename.</summary>
    private static void AssertRule(PredicateList selection, Func<Conditions, ConditionList> rule)
    {
        selection.GetTypes().Should().NotBeEmpty();
        var result = rule(selection.Should()).GetResult();
        (result.FailingTypeNames ?? []).Should().BeEmpty();
    }
}
