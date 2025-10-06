using FluxoDeCaixa.Caixa.Domain.Entities;
using FluxoDeCaixa.Caixa.Domain.Enums;
using FluxoDeCaixa.Caixa.Domain.Exceptions;
using System;
using Xunit;

namespace FluxoDeCaixa.Caixa.Domain.Tests
{
    public class AccountTests
    {
        [Fact(DisplayName = "Adicionar um valor de crédito válido")]
        [Trait("Categoria", "Conta")]
        public void Lancamento_ValorDeCreditoValido_DeveRetornarSucesso()
        {
            // Arrange
            Account account = new Account(0, DateTime.Now);
            Entry entry = new Entry(EntryType.Credit, Guid.NewGuid(), 100, "Compra do protudo 1", Guid.NewGuid());

            // Act
            account.UpdateValue(entry);

            // Assert
            Assert.Equal(100, account.Value);
        }

        [Fact(DisplayName = "Adicionar um valor de crédito inválido")]
        [Trait("Categoria", "Conta")]
        public void Lancamento_ValorDeCreditoInvalido_DeveRetornarException()
        {
            // Arrange
            Account account = new Account(0, DateTime.Now);
            Entry entry = new Entry(EntryType.Credit, Guid.NewGuid(), -100, "Compra do protudo 1", Guid.NewGuid());

            // Act e Assert
            Assert.Throws<InvalidCreditException>(() => account.UpdateValue(entry));
        }

        [Fact(DisplayName = "Adicionar um valor de débito válido")]
        [Trait("Categoria", "Conta")]
        public void Lancamento_ValorDeDebitoValido_DeveRetornarSucesso()
        {
            // Arrange
            Account account = new Account(250, DateTime.Now);
            Entry entry = new Entry(EntryType.Debit, Guid.NewGuid(), 100, "Devolução do produto 1", Guid.NewGuid());

            // Act
            account.UpdateValue(entry);

            // Assert
            Assert.Equal(150, account.Value);
        }

        [Fact(DisplayName = "Adicionar um valor inválido de débito")]
        [Trait("Categoria", "Conta")]
        public void Lancamento_ValorDeDebitoInvalido_DeveRetornarException()
        {
            // Arrange
            Account account = new Account(250, DateTime.Now);
            Entry entry = new Entry(EntryType.Debit, Guid.NewGuid(), -100, "Devolução do produto 1", Guid.NewGuid());

            // Act e Assert
            Assert.Throws<InvalidDebitException>(() => account.UpdateValue(entry));
        }

        [Fact(DisplayName = "Adicionar valor de débito maior que o saldo na conta")]
        [Trait("Categoria", "Conta")]
        public void Lancamento_SaldoInsuficienteParaODebito()
        {
            // Arrange
            Account account = new Account(200, DateTime.Now);
            Entry entry = new Entry(EntryType.Debit, Guid.NewGuid(), 220, "Teste Saldo insuficiente", Guid.NewGuid());

            // Act e Assert
            Assert.Throws<InsuficientFoundsException>(() => account.UpdateValue(entry));
        }
    }
}
