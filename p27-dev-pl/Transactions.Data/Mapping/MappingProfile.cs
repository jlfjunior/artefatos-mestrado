using AutoMapper;
using Transactions.Business.Model;

namespace Transactions.Data.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Transaction, TransactionMongoMap>();
            CreateMap<TransactionMongoMap, Transaction>();
        }
    }
}
