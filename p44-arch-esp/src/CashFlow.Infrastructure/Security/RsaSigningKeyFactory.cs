using System.Security.Cryptography;

namespace CashFlow.Infrastructure.Security;

/// <summary>
/// Gera pares RSA para assinatura de tokens JWT.
/// </summary>
public static class RsaSigningKeyFactory
{
    /// <summary>
    /// Material da chave RSA gerada para assinatura JWT.
    /// </summary>
    /// <param name="KeyId">Identificador público da chave (kid).</param>
    /// <param name="PublicModulusN">Módulo RSA codificado em Base64 URL.</param>
    /// <param name="PublicExponentE">Expoente RSA codificado em Base64 URL.</param>
    /// <param name="PrivatePkcs8Pem">Chave privada PKCS#8 em PEM.</param>
    public sealed record Material(
        string KeyId,
        string PublicModulusN,
        string PublicExponentE,
        string PrivatePkcs8Pem);

    /// <summary>
    /// Cria um material de chave RSA.
    /// </summary>
    /// <param name="modulusBits">Tamanho do módulo RSA.</param>
    /// <returns>Material da chave RSA.</returns>
    public static Material Create(int modulusBits = 2048)
    {
        using var rsa = RSA.Create(modulusBits);
        var keyId = Guid.NewGuid().ToString("D");
        var parameters = rsa.ExportParameters(false);

        return new Material(
            keyId,
            Base64UrlEncode(parameters.Modulus!),
            Base64UrlEncode(parameters.Exponent!),
            rsa.ExportPkcs8PrivateKeyPem());
    }

    /// <summary>
    /// Codifica um array de bytes em Base64 URL seguro.
    /// </summary>
    /// <param name="data">Array de bytes a ser codificado.</param>
    /// <returns>String codificada em Base64 URL seguro.</returns>
    private static string Base64UrlEncode(byte[] data) =>
        Convert.ToBase64String(data)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
