using DataForge.Modules.Identity.Application.UseCases.Authentication.Authenticate.Contracts;
using FluentValidation;

namespace DataForge.Modules.Identity.Application.UseCases.Authentication.Authenticate.Validators;

public sealed class AuthenticateUserRequestValidator : AbstractValidator<AuthenticateUserRequest>
{
    public AuthenticateUserRequestValidator()
    {
        RuleFor(request => request.Email).NotEmpty().MaximumLength(254).EmailAddress();
        RuleFor(request => request.Password).NotEmpty().MaximumLength(128);
    }
}
