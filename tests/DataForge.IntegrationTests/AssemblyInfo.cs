// Tests share one database and drain one job queue; a single collection runs them sequentially,
// which keeps queue assertions deterministic.
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]
