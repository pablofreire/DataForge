-- tenant_id deliberately has no foreign key to identity.tenants: modules own their schemas and
-- reference each other only by identifier (see ADR-0001).
CREATE TABLE onboarding.projects (
    id          bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    public_id   uuid          NOT NULL,
    tenant_id   bigint        NOT NULL,
    name        varchar(120)  NOT NULL,
    description varchar(1000) NULL,
    is_active   boolean       NOT NULL DEFAULT true,
    created_at  timestamptz   NOT NULL,
    updated_at  timestamptz   NOT NULL
);

CREATE UNIQUE INDEX ix_projects_public_id ON onboarding.projects (public_id);
CREATE INDEX ix_projects_is_active ON onboarding.projects (is_active) WHERE is_active = true;
CREATE UNIQUE INDEX ix_projects_tenant_name ON onboarding.projects (tenant_id, lower(name)) WHERE is_active = true;
