using Application.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Security;

public sealed class Sha256PasswordHasher : IPasswordHasher
{
    public string Hash(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
        return Convert.ToHexString(SHA256.HashData(bytes));
    }

    public bool Verify(string value, string hash)
    {
        return string.Equals(Hash(value), hash, StringComparison.Ordinal);
    }
}
