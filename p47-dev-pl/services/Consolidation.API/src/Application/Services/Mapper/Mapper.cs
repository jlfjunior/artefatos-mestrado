using Shared.DTOs;
using Shared.Enums;

namespace Consolidation.API.Application.Services;

public static class Mapper
{
    public static ConsolidationDto MapToDto(this Consolidation.API.Domain.Models.Consolidation consolidation)
    {
        return new ConsolidationDto
        {
            Id = consolidation.Id,
            Lojista = consolidation.Lojista,
            LojistaCodigo = consolidation.Lojista.Codigo(),
            LojaNome = consolidation.Lojista.NomeLoja(),
            ConsolidationDate = consolidation.ConsolidationDate,
            DebitTotal = consolidation.DebitTotal,
            CreditTotal = consolidation.CreditTotal,
            DailyBalance = consolidation.DailyBalance,
            ProcessedCount = consolidation.ProcessedCount,
            LastUpdatedAt = consolidation.LastUpdatedAt
        };
    }
}
