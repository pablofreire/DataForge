CREATE TABLE onboarding.datasets (
    id             bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    public_id      uuid          NOT NULL,
    tenant_id      bigint        NOT NULL,
    project_id     bigint        NOT NULL REFERENCES onboarding.projects (id),
    file_name      varchar(255)  NOT NULL,
    content_sha256 char(64)      NOT NULL,
    size_bytes     bigint        NOT NULL,
    storage_path   varchar(500)  NOT NULL,
    status         varchar(20)   NOT NULL,
    row_count      bigint        NOT NULL DEFAULT 0,
    columns        text[]        NOT NULL DEFAULT '{}',
    failure_reason varchar(2000) NULL,
    imported_at    timestamptz   NULL,
    is_active      boolean       NOT NULL DEFAULT true,
    created_at     timestamptz   NOT NULL,
    updated_at     timestamptz   NOT NULL,
    CONSTRAINT ck_datasets_status CHECK (status IN ('Uploaded', 'Importing', 'Imported', 'Failed'))
);

CREATE UNIQUE INDEX ix_datasets_public_id ON onboarding.datasets (public_id);
CREATE INDEX ix_datasets_is_active ON onboarding.datasets (is_active) WHERE is_active = true;
CREATE INDEX ix_datasets_tenant_project ON onboarding.datasets (tenant_id, project_id, id DESC);

-- Idempotent uploads: identical content uploaded twice to the same project resolves to one dataset.
CREATE UNIQUE INDEX ix_datasets_project_content ON onboarding.datasets (project_id, content_sha256) WHERE is_active = true;
