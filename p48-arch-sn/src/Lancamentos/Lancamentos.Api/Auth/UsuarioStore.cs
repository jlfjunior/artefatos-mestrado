namespace Lancamentos.Api.Auth;

/// <summary>
/// Papéis de acesso do sistema. Operador atua no write side (lançamentos),
/// Gerente no read side (consolidado) e Admin nos dois.
/// </summary>
public static class Papeis
{
    public const string Operador = "Operador";
    public const string Gerente = "Gerente";
    public const string Admin = "Admin";
}

/// <summary>
/// Repositório de credenciais para demonstração. Em produção isso seria um
/// Identity Provider externo com senhas hasheadas e rotação; aqui mantenho um
/// conjunto fixo só para exercitar o login e os papéis de ponta a ponta.
/// </summary>
public static class UsuarioStore
{
    public record Usuario(string Nome, string Senha, string Papel);

    private static readonly Dictionary<string, Usuario> Usuarios = new(StringComparer.OrdinalIgnoreCase)
    {
        ["operador"] = new("operador", "Senha@123", Papeis.Operador),
        ["gerente"] = new("gerente", "Senha@123", Papeis.Gerente),
        ["admin"] = new("admin", "Senha@123", Papeis.Admin),
    };

    public static Usuario? Autenticar(string? nome, string? senha)
    {
        if (string.IsNullOrWhiteSpace(nome) || senha is null)
            return null;

        return Usuarios.TryGetValue(nome, out var usuario) && usuario.Senha == senha
            ? usuario
            : null;
    }
}
