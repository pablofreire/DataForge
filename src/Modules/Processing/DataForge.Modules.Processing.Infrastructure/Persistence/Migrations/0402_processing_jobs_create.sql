-- Durable job queue. Workers claim rows with FOR UPDATE SKIP LOCKED and own them through a lease.
CREATE TABLE processing.jobs (
    id                  bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    public_id           uuid          NOT NULL,
    tenant_id           bigint        NOT NULL,
    tenant_public_id    uuid          NOT NULL,
    job_type            varchar(100)  NOT NULL,
    resource_id         uuid          NOT NULL,
    idempotency_key     varchar(200)  NOT NULL,
    status              varchar(20)   NOT NULL,
    attempt             integer       NOT NULL DEFAULT 0,
    max_attempts        integer       NOT NULL,
    processed_items     bigint        NOT NULL DEFAULT 0,
    total_items         bigint        NULL,
    checkpoint_position bigint        NOT NULL DEFAULT 0,
    lease_owner         varchar(200)  NULL,
    lease_expires_at    timestamptz   NULL,
    run_after           timestamptz   NOT NULL,
    cancel_requested    boolean       NOT NULL DEFAULT false,
    last_error          varchar(2000) NULL,
    started_at          timestamptz   NULL,
    completed_at        timestamptz   NULL,
    is_active           boolean       NOT NULL DEFAULT true,
    created_at          timestamptz   NOT NULL,
    updated_at          timestamptz   NOT NULL,
    CONSTRAINT ck_jobs_status CHECK (status IN ('Queued', 'Running', 'Succeeded', 'Failed', 'Cancelled')),
    CONSTRAINT ck_jobs_attempts CHECK (attempt >= 0 AND max_attempts > 0)
);

CREATE UNIQUE INDEX ix_jobs_public_id ON processing.jobs (public_id);
CREATE INDEX ix_jobs_is_active ON processing.jobs (is_active) WHERE is_active = true;
CREATE UNIQUE INDEX ix_jobs_idempotency_key ON processing.jobs (idempotency_key);

-- Dispatcher access paths: due queued jobs, and running jobs whose lease may have expired.
CREATE INDEX ix_jobs_queued_run_after ON processing.jobs (run_after, id) WHERE status = 'Queued';
CREATE INDEX ix_jobs_running_lease ON processing.jobs (lease_expires_at) WHERE status = 'Running';

-- UI access paths: latest job of a resource, and recent jobs of a tenant.
CREATE INDEX ix_jobs_tenant_resource ON processing.jobs (tenant_id, resource_id, job_type, id DESC);
CREATE INDEX ix_jobs_tenant_recent ON processing.jobs (tenant_id, id DESC);
