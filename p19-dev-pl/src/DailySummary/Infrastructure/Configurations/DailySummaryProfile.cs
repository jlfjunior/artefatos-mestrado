using Application.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Infrastructure.Configurations;

public class DailySummaryProfile : Profile
{
    public DailySummaryProfile()
    {
        CreateMap<DailySummaryEntity, DailySummaryDTO>().ReverseMap();
    }
}