namespace Consolidate.Business.UseCase.GetConsolidateByDateUseCase
{
    public record GetConsolidateByDateOutputDto(DateOnly Date, double Amount, int TotalTransactions) { }
}
