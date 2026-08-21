using System.Text.Json.Serialization;

namespace Challenger.EasyFlow.Application.Features.CashBoxManagement.ListTransaction;

public readonly record struct QueryOptions(
    [property: JsonPropertyName("skip")] int Skip,
    [property: JsonPropertyName("maxPageSize")] int MaxPageSize,
    [property: JsonPropertyName("date")] DateTime Date);
