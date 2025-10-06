using Flow.Core.DTOs;
using Flow.Exceptions;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow.Application.UseCases.Transaction
{
    public class ExecuteTransactionValidator : AbstractValidator<TransactionDTO>
    {
        public ExecuteTransactionValidator()
        {
            RuleFor(_ => _.Value).NotEmpty().WithMessage(ResourceMessageError.transaction_value_empty_error);
            RuleFor(_ => _.Value).GreaterThanOrEqualTo(0).WithMessage(ResourceMessageError.transaction_value_negative_error);
            RuleFor(_ => _.Type).NotEmpty().WithMessage(ResourceMessageError.transaction_type_empty_error);
            RuleFor(_ => _.Type).IsInEnum().WithMessage(ResourceMessageError.transaction_type_invalid_error);
        }
    }
}
