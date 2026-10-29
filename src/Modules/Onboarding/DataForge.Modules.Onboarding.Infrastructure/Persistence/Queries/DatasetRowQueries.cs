namespace DataForge.Modules.Onboarding.Infrastructure.Persistence.Queries;

internal static class DatasetRowQueries
{
    public const string CopyIn = "COPY onboarding.dataset_rows (dataset_id, row_number, values) FROM STDIN (FORMAT BINARY)";

    public const string DeleteAfter = """
        DELETE FROM onboarding.dataset_rows WHERE dataset_id = @DatasetId AND row_number > @RowNumber;
        """;

    /// <summary>Resolves the dataset through its public id and tenant, so callers can never read another tenant's rows.</summary>
    public const string ResolveDatasetId = """
        SELECT id FROM onboarding.datasets WHERE tenant_id = @TenantId AND public_id = @PublicId AND is_active = true;
        """;

    /// <summary>Keyset pagination on the primary key: constant cost per page regardless of depth.</summary>
    public const string ReadBatch = """
        SELECT row_number, values
        FROM onboarding.dataset_rows
        WHERE dataset_id = @DatasetId AND row_number > @AfterRowNumber
        ORDER BY row_number
        LIMIT @BatchSize;
        """;

    /// <summary>
    /// Rows whose (trimmed, non-blank) value in a column occurs more than once. PostgreSQL arrays are 1-based.
    /// A single hash aggregate finds the duplicated values; the join returns every row holding one of them.
    /// </summary>
    public const string ReadDuplicates = """
        WITH duplicated AS (
            SELECT btrim(values[@Ordinal]) AS value
            FROM onboarding.dataset_rows
            WHERE dataset_id = @DatasetId AND NULLIF(btrim(values[@Ordinal]), '') IS NOT NULL
            GROUP BY btrim(values[@Ordinal])
            HAVING count(*) > 1
        )
        SELECT r.row_number, btrim(r.values[@Ordinal]) AS value
        FROM onboarding.dataset_rows r
        JOIN duplicated d ON d.value = btrim(r.values[@Ordinal])
        WHERE r.dataset_id = @DatasetId
        ORDER BY r.row_number;
        """;
}
