using Consolidate.Business.Events;
using Foundation.Business.DomainNotitications;
using Foundation.Business.Model;
using System;

namespace Consolidate.Business.Model
{
    public class ConsolidateTransaction : Notifiable, IEntity
    {
        public static readonly string A_Consolidation_Must_Contain_An_Id = "A consolidation must contain an Id";
        public static readonly string The_Date_Must_Be_Valid = "The date must be valid";

        public Guid Id { get; private set; }
        public DateOnly Date { get; private set; }

        private readonly List<Transaction> _transactions;
        public IReadOnlyList<Transaction> Transactions => _transactions;

        public double TotalConsolidate => TotalCredit() - TotalDebit();


        public ConsolidateTransaction()
        {
            _transactions = new List<Transaction>();
        }

        private TransactionConsolidationCreatedEvent Create(Transaction transaction)
        {
            var (transactionId, description, value, date, type) = transaction;

            Id = Guid.NewGuid();
            Date = DateOnly.FromDateTime(date);

            _transactions.Add(transaction);
            return new TransactionConsolidationCreatedEvent(transactionId, description, value, date, type);
        }

        public TransactionConsolidationCreatedEvent Create(Guid TransactionId, string Description, double Value, DateTime Date, TransactionType Type)
        { 
            var transaction = new Transaction(TransactionId, Description, Value, Date, Type);
            return Create(transaction);
        }

        private TransactionAddedInConsolidationEvent AddTransaction(Transaction transaction)
        {
            var (transactionId, description, value, date, type) = transaction;

            _transactions.Add(transaction);
            return new TransactionAddedInConsolidationEvent(transactionId, description, value, date, type);
        }
        public TransactionAddedInConsolidationEvent AddTransaction(Guid TransactionId, string Description, double Value, DateTime Date, TransactionType Type)
        {
            var transaction = new Transaction(TransactionId, Description, Value, Date, Type);
            return AddTransaction(transaction);
        }


        private double TotalDebit()
        {
            return _transactions.Where(t => t.Type == TransactionType.Debit).Sum(t => t.Value);
        }

        private double TotalCredit()
        {
            return _transactions.Where(t => t.Type == TransactionType.Credit).Sum(t => t.Value);
        }

        private void AConsolidationMustContainAnIdSpec()
        {
            if(Id == Guid.Empty) 
            {
                AddNotification(A_Consolidation_Must_Contain_An_Id);
            }
        }

        private void TheDateMustBeValidSpec()
        {
            bool isValid = Date.Year > 1989;

            if(!isValid) 
            {
                AddNotification(The_Date_Must_Be_Valid);
            }
        }

        public override void Validate()
        {
            AConsolidationMustContainAnIdSpec();
            TheDateMustBeValidSpec();
        }
    }
}