using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CashFlowControl.Core.Application.DTOs
{
    public class TransactionCreatedFailedDTO
    {
        public Guid Id { get; set; }
        /// <summary>
        /// Valor Transação
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        /// <summary>
        /// Crédito ou Débito
        /// </summary>
        [MaxLength(6, ErrorMessage = "Tamanho máxio de 6 Caracteres. Deve ser informado Credit ou Debit.")]
        public string Type { get; set; } = string.Empty;
        /// <summary>
        /// Data de Criação
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
