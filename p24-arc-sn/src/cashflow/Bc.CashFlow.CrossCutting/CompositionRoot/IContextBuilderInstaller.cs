using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Bc.CashFlow.CrossCutting.CompositionRoot;

public interface IContextBuilderInstaller
{
	void Install(
		WebApplicationBuilder builder,
		IConfiguration? configuration = null);
}