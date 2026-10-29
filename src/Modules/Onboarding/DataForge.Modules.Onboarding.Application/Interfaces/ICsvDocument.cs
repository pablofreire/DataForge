namespace DataForge.Modules.Onboarding.Application.Interfaces;

public interface ICsvDocument : IDisposable
{
    IReadOnlyList<string> Columns { get; }

    /// <summary>Enumerates data rows once, in file order, without buffering the file.</summary>
    /// <exception cref="InvalidDataException">A row is malformed (for example, an unterminated quote).</exception>
    IEnumerable<string[]> ReadRows();
}
