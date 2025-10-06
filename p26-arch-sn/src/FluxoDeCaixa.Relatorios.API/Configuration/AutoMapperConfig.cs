using AutoMapper;
using Core.Bus.Messages;
using FluxoDeCaixa.Relatorios.API.Models;
using FluxoDeCaixa.Relatorios.API.NoSQLDatabase.Collections;
using System.Collections.Generic;

namespace FluxoDeCaixa.Relatorios.API.Configuration
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            CreateMap<AccountEntryMessage, AccountEntry>().ReverseMap();
            CreateMap<AccountEntry, AccountEntryModel>().ReverseMap();
            //CreateMap<List<AccountEntry>, List<AccountEntryModel>>(); // TODO: verificar o erro aqui
        }
    }
}
