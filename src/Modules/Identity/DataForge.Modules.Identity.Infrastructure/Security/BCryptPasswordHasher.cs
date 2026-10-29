using DataForge.Modules.Identity.Application.Interfaces;

namespace DataForge.Modules.Identity.Infrastructure.Security;

/// <summary>
/// BCrypt with an explicit work factor. Raising the work factor only affects new hashes; existing
/// hashes keep verifying because the factor is encoded in the hash itself.
/// </summary>
public sealed class BCryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 11;

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);
        return BCrypt.Net.BCrypt.EnhancedHashPassword(password, WorkFactor);
    }

    public bool Verify(string password, string passwordHash)
    {
        ArgumentNullException.ThrowIfNull(password);
        ArgumentException.ThrowIfNullOrEmpty(passwordHash);

        try
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(password, passwordHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return false;
        }
    }
}
