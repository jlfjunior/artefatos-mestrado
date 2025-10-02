using Application.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Infrastructure.Configurations;

public class TransactionProfile : Profile
{
    public TransactionProfile()
    {
        CreateMap<TransactionEntity, TransactionDTO>().ReverseMap();
    }
}
