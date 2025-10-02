using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Models;
using Domain.Models.Requests;
using Domain.Models.Responses;
using Services.Interfaces;
using Domain.Utils;

namespace Services.Services
{
    public class CashEntryService : ICashEntryService
    {
        private readonly ICashEntryRepository _repository;
        private readonly IMapper _mapper;
        public CashEntryService(ICashEntryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<CreateCashEntryResponse> CreateCashEntryAsync(CreateCashEntryRequest request, CancellationToken cancellationToken)
        {
            var cashEntryToCreate = _mapper.Map<CashEntry>(request);

            var response = await _repository.CreateAsync(cashEntryToCreate);

            return _mapper.Map<CreateCashEntryResponse>(response);
        }

        public async Task<PagnatedResult<GetDailyCashEntriesResponse>> GetDailyCashEntriesAsync(GetDailyCashEntriesResquest request, CancellationToken cancellationToken)
        {
            var orderbyLambda = Utils.OrderbyToLambda<CashEntry>(request.OrderBy);
            var descending = Utils.OrderbyToDescending(request.OrderBy);

            var cashEntriesList = await _repository.GetDailyCashEntriesAsync(
                    request.PageNumber,
                    request.PageSize,
                    orderbyLambda,
                    descending,
                    cancellationToken
            );

            var result = _mapper.Map<List<GetDailyCashEntriesResponse>>(cashEntriesList);

            var resultList = new PagnatedResult<GetDailyCashEntriesResponse>(result);

            var totalEntrys = await _repository.GetTotalCount();

            resultList.Data = result;
            resultList.CurrentPage = request.PageNumber;
            resultList.PageSize = request.PageSize;
            resultList.TotalCount = totalEntrys;
            resultList.TotalPages = (totalEntrys + request.PageSize - 1) / request.PageSize;

            return resultList;
        }
    }
}
