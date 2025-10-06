using Microsoft.AspNetCore.Builder;

namespace Bc.CashFlow.CrossCutting.CompositionRoot;

public interface IContextBuilderAppConfigurator
{
	WebApplication Configure(
		WebApplication app);
}