using AutoMapper;
using Domain.Entities;
using Domain.Models.Requests;
using Domain.Models.Responses;

namespace Domain.Mapper
{
    public class CashEntryMapper : Profile
    {
        public CashEntryMapper()
        {
            CreateMap<CreateCashEntryRequest, CashEntry>();
            CreateMap<CashEntry, CreateCashEntryResponse>();
            CreateMap<CashEntry, GetDailyCashEntriesResponse>();
        }
    }
}
