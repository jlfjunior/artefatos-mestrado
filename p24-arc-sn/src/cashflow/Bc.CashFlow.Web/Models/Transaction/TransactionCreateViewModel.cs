using Bc.CashFlow.Domain.Transaction;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bc.CashFlow.Web.Models.Transaction;

public class TransactionCreateViewModel
{
	public IEnumerable<SelectListItem>? AccountsList { get; set; }
	public int Id { get; init; }
	public int? UserId { get; init; }
	public int AccountId { get; init; }
	public TransactionType TransactionType { get; init; }
	public decimal Amount { get; init; }
	public string? Description { get; init; }
}