using AutoMapper;
using Cashflow.Transactions.Application.Requests.GetTransaction;
using Cashflow.Transactions.Application.Requests.GetTransactionsSummary;
using Cashflow.Transactions.Application.Requests.MassiveCreateTransaction;
using Cashflow.Transactions.Application.Requests.SearchTransactions;
using Cashflow.Transactions.Domain.Entities;
using Cashflow.Transactions.Domain.Enums;

namespace Cashflow.Transactions.Application.Mappings
{
    public class TransactionMappingProfile : Profile
    {
        public TransactionMappingProfile()
        {
            CreateMap<Transaction, GetTransactionsSummaryResponse>()
                .ForMember(des => des.TransactionId, opt => opt.MapFrom(src => src.Id))
                .ForMember(des => des.Type, opt => opt.MapFrom(src => src.Type.ToString()));

            CreateMap<Transaction, SearchTransactionsResponse>()
                .ForMember(des => des.TransactionId, opt => opt.MapFrom(src => src.Id))
                .ForMember(des => des.Type, opt => opt.MapFrom(src => src.Type.ToString()));

            CreateMap<Transaction, GetTransactionResponse>()
                .ForMember(des => des.TransactionId, opt => opt.MapFrom(src => src.Id))
                .ForMember(des => des.Type, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(des => des.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss")))
                .ForMember(des => des.Date, opt => opt.MapFrom(src => src.Date.ToString("dd/MM/yyyy HH:mm:ss")));

            CreateMap<MassiveCreateTransactionsModel, Transaction>()
                .ForMember(des => des.Type, opt => opt.MapFrom(src => (ETransactionType)Enum.Parse(typeof(ETransactionType), src.Type)))
                .ForMember(des => des.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(des => des.Id, opt => opt.Ignore());
        }
    }
}
