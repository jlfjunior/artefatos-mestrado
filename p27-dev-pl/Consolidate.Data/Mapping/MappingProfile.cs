using AutoMapper;
using Consolidate.Business.Model;

namespace Consolidate.Data.Mapping
{
    public class MappingProfile: Profile
    {
        public MappingProfile()
        {
            CreateMap<ConsolidateTransaction, ConsolidateTransactionMap>();
            CreateMap<ConsolidateTransactionMap, ConsolidateTransaction>();
        }
    }
}
