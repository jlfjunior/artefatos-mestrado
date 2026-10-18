namespace Consolidado.Api.Auth;

/// <summary>
/// Papéis de acesso reconhecidos no read side. Espelham os emitidos pelo
/// Lançamentos (que assina o token); a consulta do consolidado é liberada para
/// Gerente e Admin.
/// </summary>
public static class Papeis
{
    public const string Operador = "Operador";
    public const string Gerente = "Gerente";
    public const string Admin = "Admin";
}
