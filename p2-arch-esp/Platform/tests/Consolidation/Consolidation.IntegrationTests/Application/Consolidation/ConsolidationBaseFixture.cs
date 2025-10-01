using Bogus;
using Consolidation.Application.Interfaces.Repository;
using Consolidation.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Consolidation.IntegrationTests.Application.Consolidation
{
    public class ConsolidationBaseFixture
    {
        public Faker Faker { get; set; }
        public IServiceProvider ServiceProvider { get; private set; }

        public ConsolidationBaseFixture()
        {
            Faker = new Faker("pt_BR");

            var services = new ServiceCollection();
            services.AddScoped<IConsolidateRepository, ConsolidateRepository>();

            ServiceProvider = services.BuildServiceProvider();
        }

    }

    [CollectionDefinition(nameof(ConsolidationBaseFixture))]
    public class ConsolidationBaseFixtureCollection : ICollectionFixture<ConsolidationBaseFixture> { }

}
