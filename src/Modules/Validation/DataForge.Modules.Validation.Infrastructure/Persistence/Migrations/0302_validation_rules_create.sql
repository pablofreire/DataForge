-- Rules belong to a project of the Onboarding module, referenced by public id (no cross-schema FK).
CREATE TABLE validation.rules (
    id                bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    public_id         uuid         NOT NULL,
    tenant_id         bigint       NOT NULL,
    project_public_id uuid         NOT NULL,
    column_name       varchar(200) NOT NULL,
    rule_type         varchar(30)  NOT NULL,
    parameters        jsonb        NOT NULL DEFAULT '{}'::jsonb,
    is_active         boolean      NOT NULL DEFAULT true,
    created_at        timestamptz  NOT NULL,
    updated_at        timestamptz  NOT NULL,
    CONSTRAINT ck_rules_rule_type CHECK (rule_type IN ('Required', 'MaxLength', 'Pattern', 'Email', 'Numeric', 'Date', 'AllowedValues', 'Unique'))
);

CREATE UNIQUE INDEX ix_rules_public_id ON validation.rules (public_id);
CREATE INDEX ix_rules_is_active ON validation.rules (is_active) WHERE is_active = true;
CREATE INDEX ix_rules_tenant_project ON validation.rules (tenant_id, project_public_id) WHERE is_active = true;
