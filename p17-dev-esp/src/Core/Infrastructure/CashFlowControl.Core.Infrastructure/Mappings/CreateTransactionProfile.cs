using AutoMapper;
using CashFlowControl.Core.Application.DTOs;
using CashFlowControl.Core.Domain.Entities;

namespace CashFlowControl.Core.Infrastructure.Mappings
{
    public class CreateTransactionProfile : Profile
    {
        public CreateTransactionProfile()
        {
            CreateMap<CreateTransactionDTO, Transaction>().ReverseMap();
        }
    }
}
