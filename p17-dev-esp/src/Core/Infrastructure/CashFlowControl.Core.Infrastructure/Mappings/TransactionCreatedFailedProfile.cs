using AutoMapper;
using CashFlowControl.Core.Application.DTOs;

namespace CashFlowControl.Core.Infrastructure.Mappings
{
    public class TransactionCreatedFailedProfile : Profile
    {
        public TransactionCreatedFailedProfile()
        {
            CreateMap<TransactionCreatedDTO, TransactionCreatedFailedDTO>().ReverseMap();
        }
    }
}
