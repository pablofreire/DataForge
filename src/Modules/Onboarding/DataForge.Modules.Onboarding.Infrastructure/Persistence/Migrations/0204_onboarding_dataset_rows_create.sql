-- Staging table for imported rows. High-volume by design (ADR-0004):
--   * no surrogate/public id and no soft delete per row; lifecycle is managed per dataset;
--   * values are stored as text[] in header order, so any CSV shape fits without DDL;
--   * the primary key (dataset_id, row_number) serves keyset pagination and checkpoint cleanup;
--   * no foreign key to datasets, which keeps binary COPY free of per-row FK checks.
CREATE TABLE onboarding.dataset_rows (
    dataset_id bigint NOT NULL,
    row_number bigint NOT NULL,
    values     text[] NOT NULL,
    CONSTRAINT pk_dataset_rows PRIMARY KEY (dataset_id, row_number)
);
