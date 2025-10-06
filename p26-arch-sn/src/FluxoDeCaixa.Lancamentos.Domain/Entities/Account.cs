using FluxoDeCaixa.Caixa.Domain.Exceptions;
using FluxoDeCaixa.Caixa.Domain.Repositories.Base;
using System;
using System.Collections.Generic;

namespace FluxoDeCaixa.Caixa.Domain.Entities
{
    public class Account : Entity, IAggregationRoot
    {
        public Account(double value, DateTime lastUpdate)
        {
            Value = value;
            LastUpdate = lastUpdate;
        }

        public double Value { get; set; }
        public DateTime LastUpdate { get; set; }

        // EF Relationship
        public List<Entry> Entries { get; set; }

        // EF Ctor
        public Account() { }       

        public void UpdateValue(Entry entry)
        {
            switch (entry.Type)
            {
                case Enums.EntryType.Credit:
                    Credit(entry.Value);
                    break;
                case Enums.EntryType.Debit:
                    Debit(entry.Value);
                    break;
            }
        }

        private void Credit(double value)
        {
            if (value > 0)
                this.Value += value;
            else
                throw new InvalidCreditException();
        }

        private void Debit(double value)
        {
            if (value < 0)
                throw new InvalidDebitException();
            else if (value <= this.Value)
                Value -= value;
            else
                throw new InsuficientFoundsException();

        }
    }
}
