using System.Security.Cryptography;
using System.Text;

namespace CashFlow.Infrastructure.Security;

/// <summary>
/// Cifra e decifra material privado PKCS#8 com AES-256-GCM.
/// </summary>
public static class SigningKeyPrivateCodec
{
    /// <summary>
    /// Cifra um material privado PKCS#8 com AES-256-GCM.
    /// </summary>
    /// <param name="privatePkcs8Pem">Material privado PKCS#8 em formato PEM.</param>
    /// <param name="masterKey">Chave mestra para criptografia.</param>
    /// <returns>Material cifrado em formato Base64 URL seguro.</returns>
    public static string Encrypt(string privatePkcs8Pem, byte[] masterKey)
    {
        var plain = Encoding.UTF8.GetBytes(privatePkcs8Pem);
        var iv = RandomNumberGenerator.GetBytes(12);
        var cipher = new byte[plain.Length];
        var tag = new byte[16];

        using var aes = new AesGcm(masterKey, tag.Length);
        aes.Encrypt(iv, plain, cipher, tag);

        return $"{Convert.ToBase64String(iv)}.{Convert.ToBase64String(cipher)}.{Convert.ToBase64String(tag)}";
    }

    /// <summary>
    /// Decifra um material cifrado com AES-256-GCM.
    /// </summary>
    /// <param name="encrypted">Material cifrado em formato Base64 URL seguro.</param>
    /// <param name="masterKey">Chave mestra para decriptografia.</param>
    /// <returns>Material privado PKCS#8 em formato PEM.</returns>
    public static string Decrypt(string encrypted, byte[] masterKey)
    {
        var parts = encrypted.Split('.', 3);
        if (parts.Length != 3)
        {
            throw new InvalidOperationException("Invalid encrypted private key format.");
        }

        var iv = Convert.FromBase64String(parts[0]);
        var cipher = Convert.FromBase64String(parts[1]);
        var tag = Convert.FromBase64String(parts[2]);
        var plain = new byte[cipher.Length];

        using var aes = new AesGcm(masterKey, tag.Length);
        aes.Decrypt(iv, cipher, tag, plain);

        return Encoding.UTF8.GetString(plain);
    }
}
