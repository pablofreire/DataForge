-- High-volume table written with binary COPY (ADR-0004): no surrogate/public id, no soft delete per row,
-- no foreign keys. The primary key doubles as the keyset-pagination index (row_number, rule_id).
-- row_number 0 holds dataset-level issues (for example, a rule whose column is missing from the file).
CREATE TABLE validation.issues (
    run_id      bigint       NOT NULL,
    row_number  bigint       NOT NULL,
    rule_id     bigint       NOT NULL,
    column_name varchar(200) NOT NULL,
    rule_type   varchar(30)  NOT NULL,
    message     varchar(500) NOT NULL,
    value       varchar(200) NULL,
    CONSTRAINT pk_issues PRIMARY KEY (run_id, row_number, rule_id)
);
