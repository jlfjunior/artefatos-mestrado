namespace CashFlow.Infrastructure.Auth;

/// <summary>
/// Material de chave RSA persistida para assinatura JWT.
/// </summary>
/// <param name="Id">Identificador interno do registro.</param>
/// <param name="KeyId">Identificador público da chave (kid).</param>
/// <param name="Algorithm">Algoritmo de assinatura (ex.: RS256).</param>
/// <param name="KeyUse">Uso da chave (ex.: sig).</param>
/// <param name="PublicModulusN">Módulo RSA codificado em Base64 URL.</param>
/// <param name="PublicExponentE">Expoente RSA codificado em Base64 URL.</param>
/// <param name="EncryptedPrivateKey">Chave privada cifrada.</param>
/// <param name="CreatedAt">Instante UTC de criação da chave.</param>
public sealed record SigningKeyRecord(
    Guid Id,
    string KeyId,
    string Algorithm,
    string KeyUse,
    string PublicModulusN,
    string PublicExponentE,
    string EncryptedPrivateKey,
    DateTime CreatedAt);
