using AutoMapper;

namespace Domain.Mapper
{
    public class AutoMapperConfig
    {
        public static MapperConfiguration RegisterMapping()
        {
            return new MapperConfiguration(x =>
            {
                x.AddProfile(new CashEntryMapper());
            });
        }

    }
}
