using System.Threading;
using System.Threading.Tasks;
using FluxoCaixa.Consolidado.Domain.Repositories;
using MediatR;

namespace FluxoCaixa.Consolidado.Application.Queries;

public class GetConsolidadoQueryHandler : IRequestHandler<GetConsolidadoQuery, ConsolidadoResponse>
{
    private readonly IConsolidadoRepository _repository;
    private readonly ICacheService _cache;

    public GetConsolidadoQueryHandler(IConsolidadoRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<ConsolidadoResponse> Handle(GetConsolidadoQuery request, CancellationToken cancellationToken)
    {
        if (!DateOnly.TryParseExact(request.Data, "yyyy-MM-dd", out var data))
            throw new ArgumentException("Formato de data inválido. Use yyyy-MM-dd");

        var cacheKey = $"consolidado:{request.Data}";
        var cachedResponse = await _cache.GetAsync<ConsolidadoResponse>(cacheKey);

        if (cachedResponse != null)
            return cachedResponse;

        var entity = await _repository.GetByDataAsync(data, cancellationToken);

        var response = entity == null
            ? new ConsolidadoResponse(data, 0, 0, 0, 0, DateTimeOffset.UtcNow)
            : new ConsolidadoResponse(
                entity.Data,
                entity.TotalCreditos,
                entity.TotalDebitos,
                entity.SaldoConsolidado,
                entity.QuantidadeLancamentos,
                entity.UltimaAtualizacao);

        // Cache por 60s
        await _cache.SetAsync(cacheKey, response, TimeSpan.FromSeconds(60));

        return response;
    }
}
