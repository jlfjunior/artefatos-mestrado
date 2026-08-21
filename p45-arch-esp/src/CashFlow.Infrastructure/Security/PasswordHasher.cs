using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace CashFlow.Infrastructure.Security;

/// <summary>
/// Hash de senhas com PBKDF2 e prefixo versionado.
/// </summary>
public static class PasswordHasher
{
    private const string Prefix = "pw1$";
    private const int SaltBytes = 16;
    private const int KeyBytes = 64;
    private const int Iterations = 100_000;

    /// <summary>
    /// Gera um hash de uma senha em texto plano.
    /// </summary>
    /// <param name="plain">Senha em texto plano.</param>
    /// <returns>Hash da senha.</returns>
    public static string Hash(string plain)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltBytes);
        var key = KeyDerivation.Pbkdf2(
            plain,
            salt,
            KeyDerivationPrf.HMACSHA256,
            Iterations,
            KeyBytes);

        return $"{Prefix}{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    /// <summary>
    /// Verifica se uma senha é igual a uma senha armazenada.
    /// </summary>
    /// <param name="plain">Senha em texto plano.</param>
    /// <param name="stored">Senha armazenada.</param>
    /// <returns>True se a senha é igual, false caso contrário.</returns>
    public static bool Verify(string plain, string stored)
    {
        if (!stored.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return false;
        }

        var payload = stored[Prefix.Length..];
        var parts = payload.Split('.', 2);
        if (parts.Length != 2)
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[0]);
        var expected = Convert.FromBase64String(parts[1]);
        var actual = KeyDerivation.Pbkdf2(
            plain,
            salt,
            KeyDerivationPrf.HMACSHA256,
            Iterations,
            expected.Length);

        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
