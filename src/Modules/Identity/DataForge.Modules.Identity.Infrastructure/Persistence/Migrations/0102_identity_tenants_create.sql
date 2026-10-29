CREATE TABLE identity.tenants (
    id         bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    public_id  uuid         NOT NULL,
    name       varchar(200) NOT NULL,
    slug       varchar(100) NOT NULL,
    is_active  boolean      NOT NULL DEFAULT true,
    created_at timestamptz  NOT NULL,
    updated_at timestamptz  NOT NULL
);

CREATE UNIQUE INDEX ix_tenants_public_id ON identity.tenants (public_id);
CREATE INDEX ix_tenants_is_active ON identity.tenants (is_active) WHERE is_active = true;
CREATE UNIQUE INDEX ix_tenants_slug ON identity.tenants (slug);
