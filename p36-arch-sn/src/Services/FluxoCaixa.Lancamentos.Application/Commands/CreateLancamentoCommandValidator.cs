using FluentValidation;

namespace FluxoCaixa.Lancamentos.Application.Commands;

public class CreateLancamentoCommandValidator : AbstractValidator<CreateLancamentoCommand>
{
    public CreateLancamentoCommandValidator()
    {
        RuleFor(x => x.Valor)
            .GreaterThan(0)
            .WithMessage("Valor deve ser maior que zero.");

        RuleFor(x => x.Descricao)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Tipo)
            .Must(x => x.ToUpper() == "CREDITO" || x.ToUpper() == "DEBITO")
            .WithMessage("O tipo deve ser CREDITO ou DEBITO.");
    }
}
