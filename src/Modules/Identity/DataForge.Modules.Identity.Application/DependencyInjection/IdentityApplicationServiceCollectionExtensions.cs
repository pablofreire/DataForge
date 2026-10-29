using DataForge.Modules.Identity.Application.Services;
using DataForge.Modules.Identity.Application.UseCases.Authentication.Authenticate.Implementations;
using DataForge.Modules.Identity.Application.UseCases.Authentication.Authenticate.Interfaces;
using DataForge.Modules.Identity.Application.UseCases.Authentication.Authenticate.Validators;
using DataForge.Modules.Identity.Contracts.Interfaces;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DataForge.Modules.Identity.Application.DependencyInjection;

public static class IdentityApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<AuthenticateUserRequestValidator>(ServiceLifetime.Singleton);
        services.AddScoped<IAuthenticateUserUseCase, AuthenticateUserUseCase>();
        services.AddScoped<ITenantDirectory, TenantDirectory>();
        return services;
    }
}
