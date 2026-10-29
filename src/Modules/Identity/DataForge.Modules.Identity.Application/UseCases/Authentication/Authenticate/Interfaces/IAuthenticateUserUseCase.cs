using DataForge.BuildingBlocks.Application.UseCases.Interfaces;
using DataForge.Modules.Identity.Application.UseCases.Authentication.Authenticate.Contracts;

namespace DataForge.Modules.Identity.Application.UseCases.Authentication.Authenticate.Interfaces;

public interface IAuthenticateUserUseCase : IUseCase<AuthenticateUserRequest, AuthenticateUserResponse>;
