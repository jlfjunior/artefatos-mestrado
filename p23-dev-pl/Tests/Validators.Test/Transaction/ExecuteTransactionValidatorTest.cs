using Flow.Application.UseCases.Transaction;
using Flow.Core.DTOs;
using Flow.Core.Enums;
using Flow.Exceptions;

namespace Validators.Test.Transaction
{
    public class ExecuteTransactionValidatorTest
    {
        [Fact]
        public void Success()
        {
            var validator = new ExecuteTransactionValidator();
            var transaction = CreateTransactionDTO(10, TypeTransaction.Credit);

            var result = validator.Validate(transaction);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Return_Exception_When_Value_Is_Empty()
        {
            var validator = new ExecuteTransactionValidator();
            var transaction = CreateTransactionDTO(0, TypeTransaction.Credit);

            var result = validator.Validate(transaction);

            Assert.False(result.IsValid);
            Assert.Equal(ResourceMessageError.transaction_value_empty_error, result.Errors[0].ErrorMessage);
        }

        [Fact]
        public void Should_Return_Exception_When_Type_Is_Invalid()
        {
            var validator = new ExecuteTransactionValidator();
            var transaction = CreateTransactionDTO(10, (TypeTransaction)3);
            var result = validator.Validate(transaction);

            Assert.False(result.IsValid);
            Assert.Equal(ResourceMessageError.transaction_type_invalid_error, result.Errors[0].ErrorMessage);
        }

        private static TransactionDTO CreateTransactionDTO(decimal value, TypeTransaction type)
        {
            return new TransactionDTO
            {
                Value = value,
                Type = type
            };
        }
    }
}
