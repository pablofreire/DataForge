# ADR-0005: Streaming pipeline with bounded channels and binary COPY

- Status: Accepted
- Date: 2026-10-29

## Context

Customer files can be arbitrarily large. Memory must stay constant, reading must overlap with writing, and a failure must not leave partial or duplicated rows behind.

## Decision

- **Upload:** a single pass. The request body flows through `HashingReadStream`, which computes SHA-256 and enforces the size limit, and goes straight to `IFileStorage`. Nothing is buffered in memory.
- **Import and validation:** both use `BoundedChannelPipeline`.
  - A producer task fills a bounded `Channel<T>`: CSV batches for import, keyset-paginated row batches for validation.
  - A consumer drains it. When the consumer is slower, `WriteAsync` waits, which gives backpressure.
  - Memory is bounded by `capacity × batch size`, whatever the file size.
  - A failure on either side cancels the other.
- **Writes:** PostgreSQL binary COPY, one transaction per batch, followed by a checkpoint.
  - To resume, the executor first deletes rows after the checkpoint, which discards a partially written batch, and then skips the rows it already wrote.
  - An integration test proves that resuming gives byte-identical results.
- **Validation parallelism:** rows of a batch are validated with `Parallel.For`, since rows are independent. Issues are flattened in row order so that COPY writes in primary-key order.
- **Uniqueness:** set-based. One SQL hash aggregate per rule, after the streaming pass, instead of an in-memory set.

## Consequences

- Constant memory, CPU-parallel validation and the fastest client-side bulk path available on PostgreSQL.
- Batch size and channel capacity are configuration (`Onboarding:*`, `Validation:*`). The planned BenchmarkDotNet suite will tune them with measurements instead of guesses.
