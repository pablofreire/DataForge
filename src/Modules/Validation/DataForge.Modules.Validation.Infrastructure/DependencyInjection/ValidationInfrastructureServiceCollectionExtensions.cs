using Dapper;
using DataForge.BuildingBlocks.Infrastructure.Persistence.TypeHandlers;
using DataForge.Modules.Validation.Application.Interfaces;
using DataForge.Modules.Validation.Domain.Interfaces.Repositories;
using DataForge.Modules.Validation.Domain.ValueObjects;
using DataForge.Modules.Validation.Infrastructure.Persistence.Repositories;
using DataForge.Modules.Validation.Infrastructure.Persistence.Writers;
using Microsoft.Extensions.DependencyInjection;

namespace DataForge.Modules.Validation.Infrastructure.DependencyInjection;

public static class ValidationInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddValidationInfrastructure(this IServiceCollection services)
    {
        SqlMapper.AddTypeHandler(new JsonbTypeHandler<RuleParameters>());

        services.AddSingleton<IValidationRuleRepository, ValidationRuleRepository>();
        services.AddSingleton<IValidationRunRepository, ValidationRunRepository>();
        services.AddSingleton<IValidationIssueRepository, ValidationIssueRepository>();
        services.AddSingleton<IValidationIssueWriter, PostgresValidationIssueWriter>();
        return services;
    }
}
