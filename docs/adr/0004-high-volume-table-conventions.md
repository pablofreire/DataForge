# ADR-0004: Exception to the entity conventions for high-volume tables

- Status: Accepted
- Date: 2026-10-29

## Context

Every business table follows the house conventions:
- a `bigint` identity `id` and a `uuid` `public_id`;
- soft delete with `is_active`;
- `created_at` and `updated_at`;
- indexes `ix_{table}_public_id` and `ix_{table}_is_active`.

`onboarding.dataset_rows` and `validation.issues` hold one row per staged record and one row per violation, which can mean hundreds of millions of rows. For those tables the conventions would roughly double storage and index-maintenance cost, without serving any access pattern.

## Decision

The two high-volume tables are exempt.

| | Business tables | `dataset_rows`, `issues` |
|---|---|---|
| Identifier | `id` + `public_id` | Natural composite key: `(dataset_id, row_number)` and `(run_id, row_number, rule_id)` |
| Soft delete | `is_active` per row | Lifecycle managed per dataset or run |
| Foreign keys | Yes, within the module | None, so binary COPY skips per-row FK checks |
| Primary index | `id` | The composite key, which also serves keyset pagination |

Row values are stored as `text[]` in header order, so any CSV shape fits without DDL.

## Consequences

- Fast bulk loads and a single index per table.
- Deleting a dataset will mean dropping its rows in bulk. Table partitioning by dataset is the planned optimization once datasets in the hundreds of millions of rows are tested.
- An integration test checks that every table with a `public_id` has the mandatory indexes, so this exception does not spread to other tables by accident.
