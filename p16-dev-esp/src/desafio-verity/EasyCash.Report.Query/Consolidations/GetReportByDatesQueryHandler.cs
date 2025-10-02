using EasyCash.Abstractions.Messaging.Queries;
using EasyCash.Report.Query.Consolidations.Interfaces;
using FluentValidation;

namespace EasyCash.Report.Query.Consolidations;

internal sealed class GetReportByDatesQueryHandler : IQueryHandler<GetReportByDatesQuery, GetReportByDatesQueryResult>
{
    private readonly IReportConsolidationQueryStore _reportConsolidationQueryStore;

    public GetReportByDatesQueryHandler(IReportConsolidationQueryStore reportConsolidationQueryStore)
    {
        _reportConsolidationQueryStore = reportConsolidationQueryStore;
    }

    public async Task<GetReportByDatesQueryResult> Handle(GetReportByDatesQuery request, CancellationToken cancellationToken)
    {
        return await _reportConsolidationQueryStore.GetReportByDatesAsync(request, cancellationToken);
    }
}

internal sealed class GetReportByDatesQueryValidator : AbstractValidator<GetReportByDatesQuery>
{
    public GetReportByDatesQueryValidator()
    {
        RuleFor(x => x.StartDate).NotNull();
        RuleFor(x => x.EndDate).NotNull();

        RuleFor(x => x).Must(x => x.EndDate >= x.StartDate)
                       .WithMessage("EndDate cannot be less than StartDate");
    }
}

