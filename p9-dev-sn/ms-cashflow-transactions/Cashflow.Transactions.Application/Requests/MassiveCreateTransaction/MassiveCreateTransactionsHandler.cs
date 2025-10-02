using AutoMapper;
using Cashflow.Transactions.Application.EventService;
using Cashflow.Transactions.Application.Querys;
using Cashflow.Transactions.Application.Shared;
using Cashflow.Transactions.Domain.Entities;
using Cashflow.Transactions.Domain.Enums;
using Cashflow.Transactions.Domain.Events;
using CsvHelper;
using CsvHelper.Configuration;
using MediatR;
using System.Globalization;

namespace Cashflow.Transactions.Application.Requests.MassiveCreateTransaction
{
    public class MassiveCreateTransactionsHandler(ITransactionsQueryService transactionsQueryService, IEventPublisherService<CreatedTransactionEvent> eventPublisher, IMapper mapper) : IRequestHandler<MassiveCreateTransactionsRequest>
    {
        private readonly ITransactionsQueryService _transactionsQueryService = transactionsQueryService;
        private readonly IEventPublisherService<CreatedTransactionEvent> _eventPublisher = eventPublisher;
        private readonly IMapper _mapper = mapper;

        public async Task Handle(MassiveCreateTransactionsRequest request, CancellationToken cancellationToken)
        {
            var reader = new StreamReader(request.File);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                Delimiter = ","
            });

            var records = csv.GetRecords<MassiveCreateTransactionsModel>().ToList();
            var line = csv.Context.Parser.RawRow;

            var fileContent = new List<(int line, MassiveCreateTransactionsModel data)>();
            fileContent.AddRange(records.Select((record, index) => (index + 1, record)));

            fileContent.ForEach(transaction =>
            {
                if (!Enum.IsDefined(typeof(ETransactionType), transaction.data.Type))
                    throw new BusinessException($"Registro da linha: {transaction.line} - Tipo de transação inválido!");

                if (transaction.data.Category.Length < 3)
                    throw new BusinessException($"Registro da linha: {transaction.line} - Categoria inválido!");

                if (transaction.data.Amount < 0)
                    throw new BusinessException($"Registro da linha: {transaction.line} - Valor da transação inválido!");
            });

            var transactions = _mapper.Map<IEnumerable<Transaction>>(fileContent.Select(x => x.data)).ToList();

            try
            {
                await _transactionsQueryService.MassiveCreateTransactionsAsync(transactions);

                transactions.ForEach(async transaction =>
                {
                    var @event = new CreatedTransactionEvent
                    {
                        TransactionId = transaction.Id,
                        Category = transaction.Category,
                        Type = transaction.Type.ToString(),
                        Amount = transaction.Amount,
                        Date = transaction.Date
                    };

                    await _eventPublisher.PublishMessageAsync(@event);
                });
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
