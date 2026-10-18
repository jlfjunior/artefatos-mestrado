using FluentValidation;

namespace Lancamentos.Api.Endpoints;

public record RegistrarLancamentoRequest(string Tipo, decimal Valor, DateOnly Data, string? Descricao);

public class RegistrarLancamentoRequestValidator : AbstractValidator<RegistrarLancamentoRequest>
{
    private static readonly string[] TiposValidos = { "Credito", "Debito" };

    public RegistrarLancamentoRequestValidator()
    {
        RuleFor(x => x.Tipo)
            .NotEmpty().WithMessage("Informe o tipo do lançamento.")
            .Must(t => TiposValidos.Contains(t, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Tipo inválido. Use 'Credito' ou 'Debito'.");

        RuleFor(x => x.Valor)
            .GreaterThan(0).WithMessage("O valor deve ser maior que zero.");

        RuleFor(x => x.Data)
            .NotEqual(default(DateOnly)).WithMessage("Informe a data do lançamento.");

        RuleFor(x => x.Descricao)
            .MaximumLength(200).WithMessage("A descrição não pode passar de 200 caracteres.");
    }
}
