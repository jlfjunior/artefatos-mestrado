using MassTransit;
using Shared.DTOs;
using Transactions.API.Domain.Models;
using Transactions.API.src.Application.Interface;
using Transactions.API.src.Application.Services.Mapper;
using Transactions.API.src.Domain.Interfaces;

namespace Transactions.API.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint,
        ILogger<TransactionService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TransactionDto> CreateAsync(CreateTransactionRequest request, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Criando transação: {Amount} {Type}", request.Amount, request.Type);

            var type = Enum.Parse<TransactionType>(request.Type);

            var transactionDate = request.TransactionDate == default
                ? DateTime.UtcNow
                : request.TransactionDate;

            var transaction = Transaction.Create(request.Lojista, request.Amount, type, request.Description, transactionDate);

            await _unitOfWork.Transactions.AddAsync(transaction, cancellationToken);
            var test = await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Publicar
            var msg = new Shared.Contracts.TransactionCreatedEvent
            {
                TransactionId = transaction.Id,
                Lojista = transaction.Lojista,
                Amount = transaction.Amount,
                Type = transaction.Type.ToString(),
                Description = transaction.Description,
                TransactionDate = transaction.TransactionDate,
                CreatedAt = transaction.CreatedAt
            };

            await _publishEndpoint.Publish(msg, cancellationToken);
            _logger.LogInformation("Evento TransactionCreated publicado: {TransactionId}", transaction.Id);

            return Mapper.MapToDto(transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error: {ex}", ex);
            throw;
        }
    }

    public async Task<TransactionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var transaction = await _unitOfWork.Transactions.GetByIdAsync(id, cancellationToken);
        return transaction != null ? Mapper.MapToDto(transaction) : null;
    }

    public async Task<PaginationResponse<TransactionDto>> GetByDateRangeAsync(
        DateTime? startDate, DateTime? endDate, int pageNumber = 1, int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var transactions = await _unitOfWork.Transactions.GetByDateRangeAsync(
            startDate, endDate, pageNumber, pageSize, cancellationToken);

        var totalCount = await _unitOfWork.Transactions.GetTotalCountAsync(
            startDate, endDate, cancellationToken);

        return new PaginationResponse<TransactionDto>
        {
            Items = transactions.Select(Mapper.MapToDto).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (totalCount + pageSize - 1) / pageSize
        };
    }


}
