using AutoMapper;
using CashFlowControl.Core.Application.DTOs;
using CashFlowControl.Core.Domain.Entities;

namespace CashFlowControl.Core.Infrastructure.Mappings
{
    public class TransactionCreatedProfile : Profile
    {
        public TransactionCreatedProfile()
        {
            CreateMap<TransactionCreatedDTO, Transaction>().ReverseMap();
        }
    }
}
