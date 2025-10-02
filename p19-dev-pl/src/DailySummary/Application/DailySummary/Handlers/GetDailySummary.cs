using Application.DTOs;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using static Application.DailySummary.Handlers.GetDailySummary;

namespace Application.DailySummary.Handlers;

public class GetDailySummary(IApplicationDbContext _context, IMapper _mapper, IDistributedCache _cache)
    : IRequestHandler<GetDailySummaryQuery, DailySummaryDTO?>
{
    public record GetDailySummaryQuery(DateTime? Date) : IRequest<DailySummaryDTO?>;
    public async Task<DailySummaryDTO?> Handle(GetDailySummaryQuery request, CancellationToken cancellationToken)
    {
        var dateUtc = (request.Date ?? DateTime.UtcNow).ToUniversalTime().Date;
        var cacheKey = $"daily-summary:{dateUtc:yyyy-MM-dd}";

        var cachedSummary = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrWhiteSpace(cachedSummary))
            return JsonSerializer.Deserialize<DailySummaryDTO>(cachedSummary);

        var summary = await _context.DailySummaries
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Date.Date == dateUtc, cancellationToken);

        if (summary is not null)
        {
            var summaryDto = _mapper.Map<DailySummaryDTO>(summary);

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
                SlidingExpiration = TimeSpan.FromMinutes(30)
            };

            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(summaryDto), cacheOptions, cancellationToken);

            return summaryDto;
        }

        return null;
    }
}