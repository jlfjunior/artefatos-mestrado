namespace Consolidado.Infrastructure.Persistence;

/// <summary>Linha de controle de idempotência: um LancamentoId já aplicado.</summary>
public class LancamentoProcessado
{
    public Guid LancamentoId { get; set; }
    public DateTime ProcessadoEmUtc { get; set; }
}
