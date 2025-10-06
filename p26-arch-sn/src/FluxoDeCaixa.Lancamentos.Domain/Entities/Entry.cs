using FluxoDeCaixa.Caixa.Domain.Enums;
using FluxoDeCaixa.Caixa.Domain.Repositories.Base;
using System;

namespace FluxoDeCaixa.Caixa.Domain.Entities
{
    public class Entry : Entity
    {
        public Entry(EntryType type, Guid accountId, double value, string description, Guid userId)
        {
            Type = type;
            AccountId = accountId;
            Value = value;
            Description = description;
            UserId = userId;
        }

        public EntryType Type { get; set; }
        public Guid AccountId { get; set; } 
        public double Value { get; set; }
        public string Description { get; set; }
        public Guid UserId { get; set; }

        // EF Ctor
        public Entry() { }

        public User User { get; set; }
        public Account Account { get; set; }
    }
}
