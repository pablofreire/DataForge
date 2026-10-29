namespace DataForge.Modules.Validation.Infrastructure.Persistence.Queries;

internal static class ValidationIssueQueries
{
    public const string CopyIn = """
        COPY validation.issues (run_id, row_number, rule_id, column_name, rule_type, message, value) FROM STDIN (FORMAT BINARY)
        """;

    public const string DeleteAll = "DELETE FROM validation.issues WHERE run_id = @RunId;";

    public const string DeleteRowIssuesAfter = "DELETE FROM validation.issues WHERE run_id = @RunId AND row_number > @RowNumber;";

    public const string DeleteRuleIssues = "DELETE FROM validation.issues WHERE run_id = @RunId AND rule_id = @RuleId;";

    /// <summary>Row-value comparison on the primary key gives an index range scan for every page.</summary>
    public const string List = """
        SELECT row_number, rule_id, column_name, rule_type, message, value
        FROM validation.issues
        WHERE run_id = @RunId
          AND (row_number, rule_id) > (@AfterRowNumber, @AfterRuleId)
          AND (@ColumnName::varchar IS NULL OR lower(column_name) = lower(@ColumnName::varchar))
        ORDER BY row_number, rule_id
        LIMIT @Limit;
        """;
}
