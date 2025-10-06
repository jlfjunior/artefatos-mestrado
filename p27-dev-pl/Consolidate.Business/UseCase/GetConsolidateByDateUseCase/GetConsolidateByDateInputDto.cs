using MediatR;

namespace Consolidate.Business.UseCase.GetConsolidateByDateUseCase
{
    public record GetConsolidateByDateInputDto(DateOnly Date) : IRequest<GetConsolidateByDateOutputDto> { }
}
