# ADR-0003: Durable job queue in PostgreSQL instead of a message broker

- Status: Accepted
- Date: 2026-10-29

## Context

Imports and validations can run for hours on large datasets. They must survive worker crashes and deployments, run on several workers in parallel, resume from progress already made, and never run twice concurrently. The MVP already depends on PostgreSQL.

## Decision

The queue is the table `processing.jobs`.

| Concern | Mechanism |
|---|---|
| Claiming | `UPDATE … WHERE id = (SELECT … FOR UPDATE SKIP LOCKED LIMIT 1)`, so concurrent workers never claim the same job |
| Ownership | A lease that a heartbeat renews. A crashed worker's lease expires and another worker resumes the job |
| Fencing | Every state change includes `WHERE lease_owner = @WorkerId`, so a worker that lost its lease cannot overwrite progress |
| Clock | Lease and schedule timestamps use the database clock (`now()`), which removes skew between hosts (a real bug the integration tests caught) |
| Idempotency | A unique `idempotency_key`. Enqueueing the same work twice returns the existing job |
| Retry | Exponential backoff with a cap. Executors raise `NonRetryableJobException` for permanent failures, such as a malformed file |
| Resume | Executors receive the last committed `checkpoint_position` |
| Cancellation | A `cancel_requested` flag that the next heartbeat observes |
| Shutdown | The job is released back to the queue, keeping its checkpoint and without consuming an attempt |

Modules communicate through in-process integration events (`IIntegrationEventPublisher`).

## Consequences

- No broker to operate, and enqueueing commits in the same database as the business data.
- Throughput is bounded by PostgreSQL. That is ample for coarse-grained jobs (one job per dataset or run), not for millions of tiny messages.
- **When to move to a broker:** when modules are split into separate deployables, or when job volume outgrows a single database. The plan is a transactional outbox plus RabbitMQ or Azure Service Bus behind the same `IJobScheduler` and `IIntegrationEventPublisher` interfaces.
