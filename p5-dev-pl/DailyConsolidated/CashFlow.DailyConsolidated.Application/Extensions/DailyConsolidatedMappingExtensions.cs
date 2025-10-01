using CashFlow.DailyConsolidated.Application.DTOs;
using CashFlow.DailyConsolidated.Domain.Entities;
using CashFlow.Entries.Domain.Entities;

namespace CashFlow.DailyConsolidated.Application.Extensions
{
    public static class DailyConsolidatedMappingExtensions
    {
        public static DailyConsolidationOutput ToOutput(this DailyConsolidatedEntity consolidated)
        {
            return new DailyConsolidationOutput
            {
                Date = consolidated.Date,
                DailyResult = consolidated.DailyResult,
                Entries = consolidated.Entries?.Select(ToEntryOutput).ToList() ?? new List<EntryOutput>()
            };
        }

        private static EntryOutput ToEntryOutput(Entry entry)
        {
            return new EntryOutput
            {
                Id = entry.Id,
                Date = entry.Date,
                Value = entry.Value,
                Description = entry.Description,
                Type = entry.Type.ToString()
            };
        }
    }
}