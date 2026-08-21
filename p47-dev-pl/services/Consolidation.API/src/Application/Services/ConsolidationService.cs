using Consolidation.API.src.Application.Interface;
using Consolidation.API.src.Domain.Interfaces;
using MassTransit;
using Shared.Contracts;
using Shared.DTOs;

namespace Consolidation.API.Application.Services;

public class ConsolidationService : IConsolidationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ConsolidationService> _logger;

    public ConsolidationService(
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint,
        ILogger<ConsolidationService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ConsolidationDto> ProcessTransactionAsync(TransactionCreatedEvent msg, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
               "Processando transação para consolidação: {TransactionId} - {Amount} {Type}",
               msg.TransactionId, msg.Amount, msg.Type);

            var consolidationDate = msg.TransactionDate.Date;
            var consolidation = await _unitOfWork.Consolidations.GetByLojistaAsync(msg.Lojista, cancellationToken);

            if (consolidation == null)
            {
                consolidation = Consolidation.API.Domain.Models.Consolidation.Create(msg.Lojista, consolidationDate);
                consolidation.UpdateFromTransaction(msg.Amount, msg.Type);
                await _unitOfWork.Consolidations.AddAsync(consolidation, cancellationToken);
                _logger.LogInformation("Nova consolidação criada para o lojista {Lojista} na data: {Date}", msg.Lojista, consolidationDate);
            }
            else
            {
                consolidation.UpdateFromTransaction(msg.Amount, msg.Type);
                await _unitOfWork.Consolidations.UpdateAsync(consolidation, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var consolidationEvent = new ConsolidationUpdatedEvent
            {
                ConsolidationId = consolidation.Id,
                Lojista = consolidation.Lojista,
                ConsolidationDate = consolidation.ConsolidationDate,
                DebitTotal = consolidation.DebitTotal,
                CreditTotal = consolidation.CreditTotal,
                DailyBalance = consolidation.DailyBalance,
                ProcessedCount = consolidation.ProcessedCount
            };

            await _publishEndpoint.Publish(consolidationEvent, cancellationToken);
            _logger.LogInformation("Evento ConsolidationUpdated publicado: {ConsolidationId}", consolidation.Id);

            return Mapper.MapToDto(consolidation!);
        }
        catch (Exception ex)
        {
            _logger.LogError("Erro: {ex}", ex);
            throw;
        }
    }

    public async Task<ConsolidationDto?> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var consolidation = await _unitOfWork.Consolidations.GetByDateAsync(date, cancellationToken);
        return consolidation != null ? Mapper.MapToDto(consolidation) : null;
    }

    public async Task<PaginationResponse<ConsolidationDto>> GetByDateRangeAsync(
        DateTime? startDate, DateTime? endDate, int pageNumber = 1, int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        
        var start = startDate ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var end = endDate ?? DateTime.UtcNow.Date;

        var consolidations = await _unitOfWork.Consolidations.GetByDateRangeAsync(
            start, end, pageNumber, pageSize, cancellationToken);

        var totalCount = await _unitOfWork.Consolidations.GetTotalCountAsync(
            start, end, cancellationToken);

        return new PaginationResponse<ConsolidationDto>
        {
            Items = consolidations.Select(Mapper.MapToDto).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (totalCount + pageSize - 1) / pageSize
        };
    }


}
