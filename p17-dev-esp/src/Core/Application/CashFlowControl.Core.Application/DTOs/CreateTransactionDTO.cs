using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CashFlowControl.Core.Application.DTOs
{
    public class CreateTransactionDTO
    {
        [JsonIgnore]
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
        public string Type { get; set; } = null!;

        /// <summary>
        /// Data de Criação
        /// </summary>
        [JsonIgnore]
        public DateTime CreatedAt { get; set; }
    }
}
