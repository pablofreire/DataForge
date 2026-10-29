using DataForge.BuildingBlocks.Application.Tenancy;

namespace DataForge.BuildingBlocks.UnitTests.Tenancy;

public sealed class TenantContextTests
{
    [Fact]
    public void TenantId_BeforeResolution_Throws()
    {
        var context = new TenantContext();

        context.HasTenant.Should().BeFalse();
        context.Invoking(c => c.TenantId).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SetTenant_DifferentTenantInSameScope_Throws()
    {
        var context = new TenantContext();
        context.SetTenant(1, Guid.NewGuid());

        context.Invoking(c => c.SetTenant(2, Guid.NewGuid()))
            .Should().Throw<InvalidOperationException>("a scope must never switch tenants mid-request");
    }

    [Fact]
    public void SetTenant_SameTenantTwice_IsIdempotent()
    {
        var context = new TenantContext();
        var publicId = Guid.NewGuid();

        context.SetTenant(7, publicId);
        context.SetTenant(7, publicId);

        context.TenantId.Should().Be(7);
        context.TenantPublicId.Should().Be(publicId);
    }
}
