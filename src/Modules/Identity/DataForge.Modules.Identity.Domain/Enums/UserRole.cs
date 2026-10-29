namespace DataForge.Modules.Identity.Domain.Enums;

public enum UserRole
{
    /// <summary>Customer user who can manage the tenant's projects and rules.</summary>
    TenantAdmin = 0,

    /// <summary>Customer user with day-to-day access to the tenant's projects.</summary>
    TenantMember = 1,

    /// <summary>Internal implementation team member with access to the implementation console.</summary>
    ImplementationEngineer = 2,
}
