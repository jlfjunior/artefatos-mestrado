using FluentValidation;
using System;

namespace Core.Bus.Messages
{
    public class AccountEntryMessage : Message
    {
        public AccountEntryMessage(Guid userId, 
            string userName, 
            int entryType, 
            double entryValue, 
            double accountValueAfterEntry, 
            string entryDescription, 
            DateTime entryDate)
        {
            UserId = userId;
            UserName = userName;
            EntryType = entryType;
            EntryValue = entryValue;
            AccountValueAfterEntry = accountValueAfterEntry;
            EntryDescription = entryDescription;
            EntryDate = entryDate;
        }

        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public int EntryType { get; set; }
        public double EntryValue { get; set; }
        public double AccountValueAfterEntry { get; set; }
        public string EntryDescription { get; set; }
        public DateTime EntryDate { get; set; }

        public override bool IsValid()
        {
            ValidationResult = new AccountEntryValidation().Validate(this);

            return ValidationResult.IsValid;
        }

        public class AccountEntryValidation : AbstractValidator<AccountEntryMessage>
        {
            public AccountEntryValidation()
            {
                RuleFor(x => x.UserId)
                    .NotEmpty()
                    .WithMessage("O id do usuário não pode ser nulo");

                RuleFor(x => x.UserName)
                    .NotEmpty()
                    .WithMessage("O nome do usuário deve ser informado");

                RuleFor(x => x.EntryType)
                    .LessThanOrEqualTo(2)
                    .GreaterThan(0)
                    .WithMessage("O tipo de lançamento deve estar entre 1 e 2");

                RuleFor(x => x.EntryValue)
                    .GreaterThan(0)
                    .WithMessage("O valor de lançamento deve ser maior que 0");

                RuleFor(x => x.AccountValueAfterEntry)
                    .GreaterThan(0)
                    .WithMessage("O valor na conta após o lançamento deve ser maior que 0");

                RuleFor(x => x.EntryDescription)
                    .NotNull()
                    .NotEmpty()
                    .WithMessage("A descrição do lançamento deve ser informada");
            }

            protected enum EntryType
            {
                Credit = 1,
                Debit = 2
            }
        }
    }
}
