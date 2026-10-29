using DataForge.ArchitectureTests.Fixtures;
using DataForge.BuildingBlocks.Application.UseCases.Interfaces;
using DataForge.BuildingBlocks.Domain.Entities;
using DataForge.BuildingBlocks.Web.Api;
using DataForge.Modules.Processing.Contracts.Interfaces;
using DataForge.Modules.Validation.Domain.Engine.Interfaces;
using FluentValidation;
using NetArchTest.Rules;

namespace DataForge.ArchitectureTests;

/// <summary>Artifact suffixes documented in docs/engineering/coding-standards.md, enforced on every build.</summary>
public sealed class NamingConventionTests
{
    private static readonly Types AllTypes = Types.InAssemblies(SolutionAssemblies.All);

    [Fact]
    public void UseCases_EndWithUseCase() =>
        AssertSuccess(AllTypes.That().ImplementInterface(typeof(IUseCase<,>)).And().AreClasses()
            .Should().HaveNameEndingWith("UseCase"));

    [Fact]
    public void Validators_EndWithRequestValidator() =>
        AssertSuccess(AllTypes.That().Inherit(typeof(AbstractValidator<>))
            .Should().HaveNameEndingWith("RequestValidator"));

    [Fact]
    public void Entities_EndWithEntity() =>
        AssertSuccess(AllTypes.That().Inherit(typeof(Entity)).And().AreNotAbstract()
            .Should().HaveNameEndingWith("Entity"));

    [Fact]
    public void JobExecutors_EndWithJobExecutor() =>
        AssertSuccess(AllTypes.That().ImplementInterface(typeof(IJobExecutor))
            .Should().HaveNameEndingWith("JobExecutor"));

    [Fact]
    public void RuleStrategies_EndWithRuleStrategy() =>
        AssertSuccess(AllTypes.That().ImplementInterface(typeof(IRuleStrategy))
            .Should().HaveNameEndingWith("RuleStrategy"));

    [Fact]
    public void ApiControllers_EndWithControllerAndUseTheBaseController() =>
        AssertSuccess(AllTypes.That().Inherit(typeof(BaseApiController))
            .Should().HaveNameEndingWith("Controller").And().BeSealed());

    [Fact]
    public void Repositories_LiveInInfrastructureAndEndWithRepository() =>
        AssertSuccess(AllTypes.That().ResideInNamespaceEndingWith(".Persistence.Repositories").And().AreClasses()
            .Should().HaveNameEndingWith("Repository"));

    [Fact]
    public void SqlQueryClasses_EndWithQueriesAndAreInternal() =>
        AssertSuccess(AllTypes.That().ResideInNamespaceEndingWith(".Persistence.Queries")
            .Should().HaveNameEndingWith("Queries").And().NotBePublic());

    [Fact]
    public void Dapper_IsOnlyUsedByInfrastructure() =>
        AssertSuccess(AllTypes.That().HaveDependencyOn("Dapper")
            .Should().ResideInNamespaceContaining(".Infrastructure"));

    [Fact]
    public void ConcreteClasses_AreSealedByDefault() =>
        AssertSuccess(AllTypes.That().AreClasses().And().AreNotAbstract().And().AreNotStatic().And().AreNotNested()
            .And().DoNotHaveNameMatching("^<|^Result$|Program$|_Imports$|Page$|Panel$|Layout$|Strip$|Badge$|Header$|State$|Card$|Bar$|Message$|Modal$|Routes$|App$|RedirectTo$")
            .Should().BeSealed());

    private static void AssertSuccess(ConditionList conditions)
    {
        var result = conditions.GetResult();
        (result.FailingTypeNames ?? []).Should().BeEmpty();
    }
}
