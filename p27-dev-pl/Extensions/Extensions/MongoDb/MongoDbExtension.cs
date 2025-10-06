

using Foundation.Settings;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;

namespace Extensions.Extensions.MongoDb
{
    public static class MongoDbExtension
    {
        public static IServiceCollection AddMongoDb(this IServiceCollection services, AppSettings appSettings)
        {
            
            var clientSettings = MongoClientSettings.FromUrl(MongoUrl.Create(appSettings.MongoUrl));
            clientSettings.ClusterConfigurator = cb => cb.Subscribe(new DiagnosticsActivityEventSubscriber());

            services.AddSingleton<IMongoClient>(new MongoClient(clientSettings));
            return services;
        }
    }
}
