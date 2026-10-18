using Challenger.EasyFlow.Application.Common.CQRS;

namespace Challenger.EasyFlow.Application.Features.FinancialManagement.ConsolidatedDaily;

public sealed record ConsolidatedDailyCommand(DateTime Period) : Command;
