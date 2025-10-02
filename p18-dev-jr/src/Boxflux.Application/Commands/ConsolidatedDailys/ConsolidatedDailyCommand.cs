using MediatR;
using System.ComponentModel.DataAnnotations;
using System.Net;

public class ConsolidatedDailyCommand
{
    public class GetConsolidatedDailyQuery : IRequest<decimal>
    {
        [Required(ErrorMessage = " O Campo Data é obrigatorio EX: 2025-02-12 ")]
        public DateTime Date {  get; set; }
    }
}