using System.Security.Cryptography;
using System.Text;
using CashFlow.Contracts;
using CashFlow.Infrastructure.Configuration;
using CashFlow.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CashFlow.Infrastructure.Auth;

/// <summary>
/// Pool de chaves RSA, JWKS e assinatura de tokens JWT.
/// </summary>
/// <param name="scopeFactory">Fábrica de escopo para resolver dependências scoped.</param>
/// <param name="options">Opções OAuth (issuer, TTL, pool de chaves).</param>
public sealed class SigningKeysService(
    IServiceScopeFactory scopeFactory,
    IOptions<OAuthOptions> options)
{
    private readonly OAuthOptions _options = options.Value;
    private readonly SemaphoreSlim _sync = new(1, 1);
    private byte[]? _masterKey;

    /// <summary>
    /// Garante que o pool configurado de chaves habilitadas esteja disponível.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Task representando a operação assíncrona.</returns>
    public async Task EnsurePoolAsync(CancellationToken cancellationToken = default)
    {
        await _sync.WaitAsync(cancellationToken);
        try
        {
            var count = await WithRepositoryAsync(r => r.CountEnabledAsync(cancellationToken), cancellationToken);
            while (count < _options.SigningKeysPoolSize)
            {
                await CreateAndPersistKeyAsync(cancellationToken);
                count++;
            }
        }
        finally
        {
            _sync.Release();
        }
    }

    /// <summary>
    /// Retorna o conjunto de chaves públicas no formato JWKS.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Documento JWKS com chaves habilitadas.</returns>
    public async Task<JsonWebKeySetResponse> GetJwksAsync(CancellationToken cancellationToken = default)
    {
        var keys = await WithRepositoryAsync(r => r.ListEnabledAsync(cancellationToken), cancellationToken);
        return new JsonWebKeySetResponse
        {
            Keys = keys.Select(k => new JsonWebKeyResponse
            {
                KeyType = "RSA",
                KeyId = k.KeyId,
                Use = k.KeyUse,
                Algorithm = k.Algorithm,
                Modulus = k.PublicModulusN,
                Exponent = k.PublicExponentE
            }).ToList()
        };
    }

    /// <summary>
    /// Assina e emite um access token JWT para o usuário autenticado.
    /// </summary>
    /// <param name="subjectUserId">Identificador do usuário (claim sub).</param>
    /// <param name="role">Papel do usuário (claim role).</param>
    /// <param name="scope">Escopos concedidos (claim scope).</param>
    /// <param name="clientId">Client OAuth emissor (claim client_id).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Token JWT serializado e tempo de expiração em segundos.</returns>
    public async Task<(string Token, int ExpiresIn)> SignAccessTokenAsync(
        Guid subjectUserId,
        string role,
        string scope,
        string clientId,
        CancellationToken cancellationToken = default)
    {
        var keys = await WithRepositoryAsync(r => r.ListEnabledAsync(cancellationToken), cancellationToken);
        if (keys.Count == 0)
        {
            throw new InvalidOperationException("Signing keys are not available.");
        }

        var signingKey = keys[0];
        var privatePem = SigningKeyPrivateCodec.Decrypt(signingKey.EncryptedPrivateKey, GetMasterKey());
        using var rsa = RSA.Create();
        rsa.ImportFromPem(privatePem);

        var rsaSecurityKey = new RsaSecurityKey(rsa.ExportParameters(includePrivateParameters: true))
        {
            KeyId = signingKey.KeyId
        };

        var credentials = new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256);

        var now = DateTime.UtcNow;
        var expires = now.AddSeconds(_options.AccessTokenTtlSeconds);
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var token = handler.CreateJwtSecurityToken(
            issuer: _options.Issuer,
            audience: null,
            subject: new System.Security.Claims.ClaimsIdentity(
            [
                new("sub", subjectUserId.ToString("D")),
                new("role", role),
                new("scope", scope),
                new("token_use", "user"),
                new("client_id", clientId)
            ]),
            notBefore: now,
            expires: expires,
            issuedAt: now,
            signingCredentials: credentials);

        token.Header["kid"] = signingKey.KeyId;
        return (handler.WriteToken(token), _options.AccessTokenTtlSeconds);
    }

    /// <summary>
    /// Resolve a chave pública RSA usada para validar um JWT pelo kid do header.
    /// </summary>
    /// <param name="keyId">Identificador da chave (kid).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Chave de validação ou null se não encontrada.</returns>
    public async Task<SecurityKey?> ResolveValidationKeyAsync(string keyId, CancellationToken cancellationToken = default)
    {
        var keys = await WithRepositoryAsync(r => r.ListEnabledAsync(cancellationToken), cancellationToken);
        var match = keys.FirstOrDefault(k => k.KeyId == keyId);
        if (match is null)
        {
            return null;
        }

        var rsa = RSA.Create();
        rsa.ImportParameters(new RSAParameters
        {
            Modulus = Base64UrlDecode(match.PublicModulusN),
            Exponent = Base64UrlDecode(match.PublicExponentE)
        });

        return new RsaSecurityKey(rsa) { KeyId = match.KeyId };
    }

    private async Task CreateAndPersistKeyAsync(CancellationToken cancellationToken)
    {
        var material = RsaSigningKeyFactory.Create();
        var encrypted = SigningKeyPrivateCodec.Encrypt(material.PrivatePkcs8Pem, GetMasterKey());

        await WithRepositoryAsync(
            r => r.InsertAsync(
                new SigningKeyRecord(
                    Guid.NewGuid(),
                    material.KeyId,
                    "RS256",
                    "sig",
                    material.PublicModulusN,
                    material.PublicExponentE,
                    encrypted,
                    DateTime.UtcNow),
                cancellationToken),
            cancellationToken);
    }

    private async Task<T> WithRepositoryAsync<T>(
        Func<ISigningKeyRepository, Task<T>> action,
        CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISigningKeyRepository>();
        return await action(repository);
    }

    private async Task WithRepositoryAsync(
        Func<ISigningKeyRepository, Task> action,
        CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISigningKeyRepository>();
        await action(repository);
    }

    private byte[] GetMasterKey()
    {
        if (_masterKey is not null)
        {
            return _masterKey;
        }

        if (string.IsNullOrWhiteSpace(_options.SigningKeysSecret) || _options.SigningKeysSecret.Length < 32)
        {
            throw new InvalidOperationException("OAuth:SigningKeysSecret must contain at least 32 characters.");
        }

        _masterKey = SHA256.HashData(Encoding.UTF8.GetBytes(_options.SigningKeysSecret));
        return _masterKey;
    }

    private static byte[] Base64UrlDecode(string value)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }

        return Convert.FromBase64String(padded);
    }
}
