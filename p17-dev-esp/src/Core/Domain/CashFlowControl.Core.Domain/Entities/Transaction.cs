using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CashFlowControl.Core.Domain.Entities
{
    public class Transaction
    {
        /// <summary>
        /// ID único da transação
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Valor Transação
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        /// <summary>
        /// Credit ou Debit
        /// </summary>
        [MaxLength(6, ErrorMessage = "Tamanho máxio de 6 Caracteres. Deve ser informado Credit ou Debit.")]
        public string Type { get; set; } = string.Empty;
        /// <summary>
        /// Data de Criação
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
