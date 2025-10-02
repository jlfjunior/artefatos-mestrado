using AutoMapper;
using CashFlowControl.Core.Application.DTOs;

namespace CashFlowControl.Core.Infrastructure.Mappings
{
    public class CreateTransactionTransCreatedProfile : Profile
    {
        public CreateTransactionTransCreatedProfile()
        {
            CreateMap<CreateTransactionDTO, TransactionCreatedDTO>().ReverseMap();
        }
    }
}
