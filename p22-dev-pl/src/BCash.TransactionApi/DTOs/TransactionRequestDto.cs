using System.ComponentModel.DataAnnotations;

namespace BCash.TransactionApi.DTOs
{
    public class TransactionRequestDto
    {
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        [RegularExpression("D|C", ErrorMessage = "The type must be either 'C' or 'D'.")]
        public required string Type { get; set; }

        public string? Description { get; set; }
    }
}
