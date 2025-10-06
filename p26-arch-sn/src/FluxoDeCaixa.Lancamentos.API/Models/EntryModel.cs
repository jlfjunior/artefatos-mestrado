using System.ComponentModel.DataAnnotations;

namespace FluxoDeCaixa.Caixa.Models
{
    public class EntryModel
    {
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public int Type { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [Range(1, 1000000, ErrorMessage = "O campo {0} deve estar entre {1} e {2}")]
        public double Value { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public string Description { get; set; }        
    }
}
