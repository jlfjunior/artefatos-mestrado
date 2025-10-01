using Consolidation.Application.Interfaces.Repository;
using Consolidation.Application.UseCases.Consolidation.Get;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Consolidation.IntegrationTests.Application.Consolidation.Get
{
    [Collection(nameof(ConsolidationBaseFixture))]

    public class GetConsolidationsTests
    {
        private readonly ConsolidationBaseFixture _fixture;
        private readonly IConsolidateRepository _repository;

        public GetConsolidationsTests(ConsolidationBaseFixture fixture)
        {
            _fixture = fixture;
            _repository = fixture.ServiceProvider.GetRequiredService<IConsolidateRepository>();
        }

        [Fact]
        public async Task Handle_ShouldReturnContacts_FromDatabase()
        {
            // Arrange
            var useCase = new GetConsolidate(_repository);
            var input = new GetConsolidateInput();

            // Act
            var result = await useCase.Handle(input, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().NotBeNull();
        }

    }
}
