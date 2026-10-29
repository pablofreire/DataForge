using System.Reflection;
using System.Text;
using DataForge.BuildingBlocks.Application.Storage.Interfaces;
using DataForge.BuildingBlocks.Application.Tenancy;
using DataForge.Modules.Onboarding.Application.Options;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.Upload.Contracts;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.Upload.Implementations;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.Upload.Validators;
using DataForge.Modules.Onboarding.Domain.Entities;
using DataForge.Modules.Onboarding.Domain.Errors;
using DataForge.Modules.Onboarding.Domain.Interfaces.Repositories;
using DataForge.Modules.Processing.Contracts.Dtos;
using DataForge.Modules.Processing.Contracts.Interfaces;
using NSubstitute;

namespace DataForge.Modules.Onboarding.UnitTests.UseCases.Datasets.Upload.Implementations;

public sealed class UploadDatasetUseCaseTests
{
    private readonly IProjectRepository _projects = Substitute.For<IProjectRepository>();
    private readonly IDatasetRepository _datasets = Substitute.For<IDatasetRepository>();
    private readonly IFileStorage _storage = Substitute.For<IFileStorage>();
    private readonly IJobScheduler _scheduler = Substitute.For<IJobScheduler>();
    private readonly TenantContext _tenant = new();
    private readonly ProjectEntity _project;

    public UploadDatasetUseCaseTests()
    {
        _tenant.SetTenant(5, Guid.NewGuid());
        _project = ProjectEntity.Create(5, "Customers", null, DateTimeOffset.UtcNow);
        typeof(ProjectEntity).GetProperty(nameof(ProjectEntity.Id), BindingFlags.Instance | BindingFlags.Public)!.SetValue(_project, 77L);

        _projects.GetByPublicIdAsync(5, _project.PublicId, Arg.Any<CancellationToken>()).Returns(_project);
        _datasets.AddAsync(default!, default).ReturnsForAnyArgs(true);
        _scheduler.EnqueueAsync(default!, default, default!, default).ReturnsForAnyArgs(call => Job(call.ArgAt<Guid>(1)));

        // The storage consumes the stream, which drives hashing and size limiting.
        _storage.WriteAsync(default!, default!, default).ReturnsForAnyArgs(call => call.ArgAt<Stream>(1).CopyToAsync(Stream.Null));
    }

    [Fact]
    public async Task ExecuteAsync_NewContent_StoresDatasetAndEnqueuesIdempotentImport()
    {
        var result = await CreateUseCase().ExecuteAsync(Request("id,name\n1,Alice\n"), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsDuplicate.Should().BeFalse();
        await _datasets.Received(1).AddAsync(Arg.Is<DatasetEntity>(d => d.ProjectId == 77 && d.TenantId == 5 && d.ContentSha256.Length == 64), Arg.Any<CancellationToken>());
        await _scheduler.Received(1).EnqueueAsync("onboarding.dataset.import", result.Value.Dataset.DatasetId, $"onboarding.dataset.import:{result.Value.Dataset.DatasetId:N}", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_SameContentUploadedAgain_ReturnsExistingDatasetAndDiscardsNewFile()
    {
        var existing = DatasetEntity.Create(5, 77, _project.PublicId, Guid.NewGuid(), "first.csv", new string('a', 64), 10, "tenants/a/b.csv", DateTimeOffset.UtcNow);
        _datasets.GetByContentHashAsync(5, 77, Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(existing);

        var result = await CreateUseCase().ExecuteAsync(Request("id\n1\n"), TestContext.Current.CancellationToken);

        result.Value.IsDuplicate.Should().BeTrue();
        result.Value.Dataset.DatasetId.Should().Be(existing.PublicId);
        await _datasets.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await _storage.Received(1).DeleteAsync(Arg.Is<string>(path => path != existing.StoragePath), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_FileAboveLimit_FailsAndRemovesPartialFile()
    {
        var result = await CreateUseCase(maxUploadBytes: 10).ExecuteAsync(Request(new string('x', 100)), TestContext.Current.CancellationToken);

        result.Error.Code.Should().Be("dataset.file_too_large");
        await _storage.ReceivedWithAnyArgs(1).DeleteAsync(default!, default);
        await _datasets.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyFile_Fails()
    {
        var result = await CreateUseCase().ExecuteAsync(Request(string.Empty), TestContext.Current.CancellationToken);

        result.Error.Should().Be(DatasetErrors.EmptyFile);
    }

    [Fact]
    public async Task ExecuteAsync_ProjectOfAnotherTenant_IsNotFound()
    {
        var result = await CreateUseCase().ExecuteAsync(
            new UploadDatasetRequest(Guid.NewGuid(), "a.csv", new MemoryStream([1])),
            TestContext.Current.CancellationToken);

        result.Error.Should().Be(ProjectErrors.NotFound);
        await _storage.DidNotReceiveWithAnyArgs().WriteAsync(default!, default!, default);
    }

    private UploadDatasetUseCase CreateUseCase(long maxUploadBytes = 1_000_000) =>
        new(
            _projects,
            _datasets,
            _storage,
            _scheduler,
            _tenant,
            new UploadDatasetRequestValidator(),
            Microsoft.Extensions.Options.Options.Create(new OnboardingOptions { MaxUploadBytes = maxUploadBytes }),
            TimeProvider.System);

    private UploadDatasetRequest Request(string content) =>
        new(_project.PublicId, "customers.csv", new MemoryStream(Encoding.UTF8.GetBytes(content)));

    private static JobSummaryDto Job(Guid resourceId) =>
        new(Guid.NewGuid(), "onboarding.dataset.import", resourceId, "Queued", 0, 3, 0, null, null, DateTimeOffset.UtcNow, null, null);
}
