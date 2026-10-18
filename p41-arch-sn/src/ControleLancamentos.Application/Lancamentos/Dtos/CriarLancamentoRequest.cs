using System.ComponentModel.DataAnnotations;
using ControleLancamentos.Domain.Lancamentos;

namespace ControleLancamentos.Application.Lancamentos.Dtos;

/// <summary>
/// Dados necessários para criar um lançamento.
/// </summary>
public sealed class CriarLancamentoRequest
{
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal Valor { get; init; }

    [EnumDataType(typeof(TipoLancamento))]
    public TipoLancamento Tipo { get; init; }

    [Required]
    public DateTime DataLancamento { get; init; }

    [StringLength(200)]
    public string? Descricao { get; init; }
}
