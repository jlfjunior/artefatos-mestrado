using Cashflow.Management.Workers.EventWorkers;

namespace Cashflow.Management.Api.Bootstrap
{
    public static class WorkerConfiguration
    {
        public static IServiceCollection ConfigureWorker(this IServiceCollection services)
        {
            services.AddHostedService<CreatedTransactionConsumerWorker>();
            services.AddHostedService<DeletedTransactionConsumerWorker>();
            services.AddHostedService<EditedTransactionConsumerWorker>();

            return services;
        }
    }
}
