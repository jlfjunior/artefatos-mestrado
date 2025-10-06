using Consolidate.Business.Contracts;
using Foundation.Business.DomainNotitications;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Consolidate.Business.UseCase.GetConsolidateByDateUseCase
{
    public class GetConsolidateByDateUseCase : IRequestHandler<GetConsolidateByDateInputDto, GetConsolidateByDateOutputDto>
    {
        private readonly ILogger<GetConsolidateByDateUseCase> _logger;
        private readonly DomainNotificationContext _domainNotificaionContext;
        private readonly IConsolidateTransactionRepository _consolidateTransactionRepository;

        public GetConsolidateByDateUseCase(
            ILogger<GetConsolidateByDateUseCase> logger,
            DomainNotificationContext domainNotificaionContext, 
            IConsolidateTransactionRepository consolidateTransactionRepository)
        {
            _logger = logger;
            _domainNotificaionContext = domainNotificaionContext;
            _consolidateTransactionRepository = consolidateTransactionRepository;
        }
        public async Task<GetConsolidateByDateOutputDto> Handle(GetConsolidateByDateInputDto request, CancellationToken cancellationToken)
        {
            GetConsolidateByDateOutputDto result = new GetConsolidateByDateOutputDto(request.Date, 0, 0); 
            try
            {
                var consolidate = await _consolidateTransactionRepository.Get(request.Date, cancellationToken);

                if (consolidate != null)
                    result = new GetConsolidateByDateOutputDto(consolidate.Date, consolidate.TotalConsolidate, consolidate.Transactions.Count);
            }
            catch (Exception e)
            {                
                _domainNotificaionContext.AddError(e);
            }

            return result;
        }
    }
}
