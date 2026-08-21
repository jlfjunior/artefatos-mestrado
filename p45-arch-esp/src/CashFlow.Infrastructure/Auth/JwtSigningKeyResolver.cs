using Microsoft.IdentityModel.Tokens;

namespace CashFlow.Infrastructure.Auth;

/// <summary>
/// Resolve chaves públicas RSA para validação de JWT pelo kid do header.
/// </summary>
/// <param name="signingKeys">Serviço de chaves de assinatura.</param>
public sealed class JwtSigningKeyResolver(SigningKeysService signingKeys)
{
    /// <summary>
    /// Resolve as chaves de validação correspondentes ao kid informado.
    /// </summary>
    /// <param name="keyId">Identificador da chave (kid) do header JWT.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista de chaves encontradas (vazia se kid ausente ou desconhecido).</returns>
    public async Task<IReadOnlyList<SecurityKey>> ResolveAsync(string? keyId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keyId))
        {
            return [];
        }

        var key = await signingKeys.ResolveValidationKeyAsync(keyId, cancellationToken);
        return key is null ? [] : [key];
    }
}
