using DataForge.Modules.Onboarding.Domain.Entities;
using DataForge.Modules.Onboarding.Domain.Enums;

namespace DataForge.Modules.Onboarding.UnitTests.Domain.Entities;

public sealed class DatasetEntityTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 29, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_StartsAsUploaded()
    {
        var dataset = NewDataset();

        dataset.Status.Should().Be(DatasetStatus.Uploaded);
        dataset.RowCount.Should().Be(0);
    }

    [Fact]
    public void ImportLifecycle_UploadedToImporting_ToImported()
    {
        var dataset = NewDataset();

        dataset.StartImport(Now);
        dataset.CompleteImport(1_000, Now.AddMinutes(1));

        dataset.Status.Should().Be(DatasetStatus.Imported);
        dataset.RowCount.Should().Be(1_000);
        dataset.ImportedAt.Should().Be(Now.AddMinutes(1));
    }

    [Fact]
    public void StartImport_WhenResumingAnInterruptedImport_IsAllowed()
    {
        var dataset = NewDataset();
        dataset.StartImport(Now);

        dataset.Invoking(d => d.StartImport(Now)).Should().NotThrow("a retried job restarts an import that was in progress");
    }

    [Fact]
    public void CompleteImport_WithoutStarting_Throws() =>
        NewDataset().Invoking(d => d.CompleteImport(1, Now)).Should().Throw<InvalidOperationException>();

    [Fact]
    public void StartImport_AfterFailure_Throws()
    {
        var dataset = NewDataset();
        dataset.Fail("bad header", Now);

        dataset.Invoking(d => d.StartImport(Now)).Should().Throw<InvalidOperationException>();
    }

    private static DatasetEntity NewDataset() =>
        DatasetEntity.Create(1, 2, Guid.NewGuid(), Guid.NewGuid(), "customers.csv", new string('a', 64), 1_024, "tenants/x/datasets/y.csv", Now);
}
