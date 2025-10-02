using EasyCash.Abstractions.Exceptions;
using EasyCash.Abstractions.Interfaces;
using EasyCash.Abstractions.Messaging.Commands;
using EasyCash.Report.Domain.Consolidations.Entity;
using EasyCash.Report.Domain.Consolidations.Errors;
using EasyCash.Report.Domain.Consolidations.Interfaces;
using EasyCash.Report.Domain.Consolidations.Models;
using FluentValidation;

namespace EasyCash.Report.Command.Consolidations.DailyConsolidations;

internal sealed class DailyConsolidationCommandHandler : ICommandHandler<DailyConsolidationCommand, DailyConsolidationCommandResult>
{
    private readonly IDailyConsolidationCommandStore _dailyConsolidationCommandStore;
    private readonly IConsolidationTransactionCommandStore _consolidationTransactionCommandStore;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DailyConsolidationCommandHandler(IDailyConsolidationCommandStore dailyConsolidationCommandStore
        , IConsolidationTransactionCommandStore consolidationTransactionCommandStore
        , IUnitOfWork unitOfWork
        , IDateTimeProvider dateTimeProvider)
    {
        _dailyConsolidationCommandStore = dailyConsolidationCommandStore;
        _consolidationTransactionCommandStore = consolidationTransactionCommandStore;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<DailyConsolidationCommandResult> Handle(DailyConsolidationCommand request, CancellationToken cancellationToken)
    {
        await Validate(request, cancellationToken);

        var transactions = await _consolidationTransactionCommandStore.GetDailyConsolidationAsync(request.Date, cancellationToken);

        var entity = MapToEntity(request.Date, transactions);

        await Save(entity, cancellationToken);

        return DailyConsolidationCommandResult.Ok(entity.Id);
    }

    private async Task Validate(DailyConsolidationCommand request, CancellationToken cancellationToken)
    {
        if (request.Date > DateOnly.FromDateTime(_dateTimeProvider.UtcNow))
            throw new BusinessException(ConsolidationsErrors.ConsolidationDateCannotBeInFuture);

        var (isAlreadyConsolidatedDay, id) = await _dailyConsolidationCommandStore.IsAlreadyConsolidatedDayAsync(request.Date, cancellationToken);
        if (isAlreadyConsolidatedDay)
            throw new BusinessException(ConsolidationsErrors.AlreadyConsolidatedDay);
    }

    private async Task Save(DailyConsolidationEntity entity, CancellationToken cancellationToken)
    {
        var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        await _dailyConsolidationCommandStore.AddAsync(entity, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        transaction.Commit();
    }

    private DailyConsolidationEntity MapToEntity(DateOnly date, DailyConsolidationModel transactions) =>
        DailyConsolidationEntity.Create(transactions.Date, transactions.TotalCredit, transactions.TotalDebit);
}

internal sealed class DailyConsolidationCommandValidator : AbstractValidator<DailyConsolidationCommand>
{
    public DailyConsolidationCommandValidator()
    {
        RuleFor(x => x.Date)
            .NotNull();
    }
}