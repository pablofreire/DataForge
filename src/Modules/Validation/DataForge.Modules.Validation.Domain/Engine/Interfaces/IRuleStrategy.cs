namespace DataForge.Modules.Validation.Domain.Engine.Interfaces;

/// <summary>
/// Row-level check for a single value. Implementations are immutable and thread-safe, because one
/// instance validates many rows in parallel.
/// </summary>
public interface IRuleStrategy
{
    /// <summary>Returns a user-facing message when <paramref name="value"/> violates the rule; otherwise null.</summary>
    string? Check(string? value);
}
