using Bc.CashFlow.Domain.QueueContext;
using Bc.CashFlow.IO.CacheContext;
using Bc.CashFlow.IO.QueueContext;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace Bc.CashFlow.CrossCutting.CompositionRoot;

// ReSharper disable once InconsistentNaming
public class IOQueueContextBuilder : IContextBuilderInstaller
{
	public void Install(
		WebApplicationBuilder builder,
		IConfiguration? configuration = null)
	{
		builder.Services.AddSingleton<IQueueConnectionFactory, QueueConnectionFactory>();
		builder.Services.AddSingleton<IConnection>(
			sp =>
				sp.GetRequiredService<IQueueConnectionFactory>().CreateConnection());
		builder.Services.AddScoped<IQueuePublisher, QueuePublisher>();
		builder.Services.AddScoped<IQueueConsumer, QueueConsumer>();
		builder.Services.AddScoped<IQueueContext, QueueContext>();
	}
}
