CREATE TABLE validation.runs (
    id                bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    public_id         uuid          NOT NULL,
    tenant_id         bigint        NOT NULL,
    dataset_public_id uuid          NOT NULL,
    project_public_id uuid          NOT NULL,
    status            varchar(20)   NOT NULL,
    total_rows        bigint        NOT NULL DEFAULT 0,
    validated_rows    bigint        NOT NULL DEFAULT 0,
    invalid_rows      bigint        NOT NULL DEFAULT 0,
    issue_count       bigint        NOT NULL DEFAULT 0,
    rule_count        integer       NOT NULL DEFAULT 0,
    failure_reason    varchar(2000) NULL,
    started_at        timestamptz   NULL,
    completed_at      timestamptz   NULL,
    is_active         boolean       NOT NULL DEFAULT true,
    created_at        timestamptz   NOT NULL,
    updated_at        timestamptz   NOT NULL,
    CONSTRAINT ck_runs_status CHECK (status IN ('Queued', 'Running', 'Completed', 'Failed'))
);

CREATE UNIQUE INDEX ix_runs_public_id ON validation.runs (public_id);
CREATE INDEX ix_runs_is_active ON validation.runs (is_active) WHERE is_active = true;
CREATE INDEX ix_runs_tenant_dataset ON validation.runs (tenant_id, dataset_public_id, id DESC);

-- Per-rule breakdown, computed once when a run completes so result pages never aggregate issues on read.
CREATE TABLE validation.run_rule_stats (
    run_id      bigint       NOT NULL REFERENCES validation.runs (id),
    rule_id     bigint       NOT NULL REFERENCES validation.rules (id),
    column_name varchar(200) NOT NULL,
    rule_type   varchar(30)  NOT NULL,
    issue_count bigint       NOT NULL,
    CONSTRAINT pk_run_rule_stats PRIMARY KEY (run_id, rule_id)
);
