CREATE TABLE identity.users (
    id             bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    public_id      uuid         NOT NULL,
    tenant_id      bigint       NOT NULL REFERENCES identity.tenants (id),
    email          varchar(254) NOT NULL,
    display_name   varchar(200) NOT NULL,
    password_hash  varchar(200) NOT NULL,
    role           varchar(40)  NOT NULL,
    last_login_at  timestamptz  NULL,
    is_active      boolean      NOT NULL DEFAULT true,
    created_at     timestamptz  NOT NULL,
    updated_at     timestamptz  NOT NULL,
    CONSTRAINT ck_users_role CHECK (role IN ('TenantAdmin', 'TenantMember', 'ImplementationEngineer')),
    CONSTRAINT ck_users_email_normalized CHECK (email = lower(btrim(email)))
);

CREATE UNIQUE INDEX ix_users_public_id ON identity.users (public_id);
CREATE INDEX ix_users_is_active ON identity.users (is_active) WHERE is_active = true;
CREATE UNIQUE INDEX ix_users_email ON identity.users (email);
CREATE INDEX ix_users_tenant_id ON identity.users (tenant_id);
