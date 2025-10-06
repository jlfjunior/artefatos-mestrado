using FluxoDeCaixa.Caixa.Domain.Entities;
using FluxoDeCaixa.Caixa.Models;
using AutoMapper;

namespace FluxoDeCaixa.Caixa.Configuration
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            CreateMap<EntryModel, Entry>().ReverseMap();
            CreateMap<AccountModel, Account>().ReverseMap();
        }
    }
}
