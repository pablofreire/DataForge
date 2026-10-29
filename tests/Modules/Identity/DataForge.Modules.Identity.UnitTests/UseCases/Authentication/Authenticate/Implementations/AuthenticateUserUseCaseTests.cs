using DataForge.Modules.Identity.Application.Interfaces;
using DataForge.Modules.Identity.Application.UseCases.Authentication.Authenticate.Contracts;
using DataForge.Modules.Identity.Application.UseCases.Authentication.Authenticate.Implementations;
using DataForge.Modules.Identity.Application.UseCases.Authentication.Authenticate.Validators;
using DataForge.Modules.Identity.Domain.Entities;
using DataForge.Modules.Identity.Domain.Enums;
using DataForge.Modules.Identity.Domain.Errors;
using DataForge.Modules.Identity.Domain.Interfaces.Repositories;
using NSubstitute;

namespace DataForge.Modules.Identity.UnitTests.UseCases.Authentication.Authenticate.Implementations;

public sealed class AuthenticateUserUseCaseTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly ITenantRepository _tenants = Substitute.For<ITenantRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly TenantEntity _tenant = TenantEntity.Create("Acme Corporation", "acme", DateTimeOffset.UtcNow);
    private readonly UserEntity _user;

    public AuthenticateUserUseCaseTests()
    {
        _user = UserEntity.Create(1, "Admin@Acme.demo", "Alice", "hash", UserRole.TenantAdmin, DateTimeOffset.UtcNow);
        _users.GetByEmailAsync("admin@acme.demo", Arg.Any<CancellationToken>()).Returns(_user);
        _tenants.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(_tenant);
        _hasher.Hash(Arg.Any<string>()).Returns("dummy-hash");
    }

    [Fact]
    public async Task ExecuteAsync_ValidCredentials_ReturnsPrincipalAndRecordsLogin()
    {
        _hasher.Verify("correct-password", "hash").Returns(true);

        var result = await CreateUseCase().ExecuteAsync(new AuthenticateUserRequest(" ADMIN@acme.demo ", "correct-password"), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("admin@acme.demo");
        result.Value.Role.Should().Be("TenantAdmin");
        result.Value.TenantName.Should().Be("Acme Corporation");
        await _users.Received(1).UpdateLastLoginAsync(_user.Id, Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WrongPassword_ReturnsGenericError()
    {
        var result = await CreateUseCase().ExecuteAsync(new AuthenticateUserRequest("admin@acme.demo", "wrong"), TestContext.Current.CancellationToken);

        result.Error.Should().Be(UserErrors.InvalidCredentials);
        await _users.DidNotReceiveWithAnyArgs().UpdateLastLoginAsync(default, default, default);
    }

    [Fact]
    public async Task ExecuteAsync_UnknownEmail_StillVerifiesAHashToEqualizeTiming()
    {
        var result = await CreateUseCase().ExecuteAsync(new AuthenticateUserRequest("nobody@acme.demo", "whatever"), TestContext.Current.CancellationToken);

        result.Error.Should().Be(UserErrors.InvalidCredentials, "the response must not reveal whether the account exists");
        _hasher.Received(1).Verify("whatever", Arg.Any<string>());
    }

    [Fact]
    public async Task ExecuteAsync_MalformedInput_ReturnsSameGenericError()
    {
        var result = await CreateUseCase().ExecuteAsync(new AuthenticateUserRequest("not-an-email", ""), TestContext.Current.CancellationToken);

        result.Error.Should().Be(UserErrors.InvalidCredentials);
    }

    [Fact]
    public void Request_ToString_NeverContainsThePassword() =>
        new AuthenticateUserRequest("a@b.c", "s3cret!").ToString().Should().NotContain("s3cret!");

    private AuthenticateUserUseCase CreateUseCase() =>
        new(_users, _tenants, _hasher, new AuthenticateUserRequestValidator(), TimeProvider.System);
}
