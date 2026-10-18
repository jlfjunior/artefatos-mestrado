namespace Shared.Enums;

/// <summary>
/// Lojistas (lojas) participantes do fluxo de caixa.
/// O valor numérico é o código do lojista (formatado como "0001".."0004")
/// e cada item representa uma loja com um nome.
/// </summary>
public enum Lojista
{
    LojaNorte = 1, // 0001
    LojaSul = 2,   // 0002
    LojaLeste = 3, // 0003
    LojaOeste = 4  // 0004
}

public static class LojistaExtensions
{
    /// <summary>Código do lojista no formato "0001".."0004".</summary>
    public static string Codigo(this Lojista lojista) => ((int)lojista).ToString("D4");

    /// <summary>Nome amigável da loja (ex.: "Loja Norte").</summary>
    public static string NomeLoja(this Lojista lojista) => lojista switch
    {
        Lojista.LojaNorte => "Loja Norte",
        Lojista.LojaSul => "Loja Sul",
        Lojista.LojaLeste => "Loja Leste",
        Lojista.LojaOeste => "Loja Oeste",
        _ => lojista.ToString()
    };
}
