using AutoMapper;
using Core.Bus;
using Core.Bus.Messages;
using FluxoDeCaixa.Relatorios.API.NoSQLDatabase.Collections;
using FluxoDeCaixa.Relatorios.API.NoSQLDatabase.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FluxoDeCaixa.Relatorios.API.BackGroundServices
{
    public class AccountEntryIntegrationHandler : BackgroundService
    {
        private readonly IMessageBus _bus;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMapper _mapper;

        public AccountEntryIntegrationHandler(IMessageBus bus, IServiceProvider serviceProvider, IMapper mapper)
        {
            _bus = bus;
            _serviceProvider = serviceProvider;
            _mapper = mapper;
        }

        private void SetSubscriber()
        {
            _bus.SubscribeAsync<AccountEntryMessage>("Account_Entry", async message => 
                await SaveEntryOnConsultDB(message));
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            SetSubscriber();
            return Task.CompletedTask;
        }

        private async Task SaveEntryOnConsultDB(AccountEntryMessage message)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var accountEntryRepository = scope.ServiceProvider.GetRequiredService<IAccountEntryRepository>();
                await accountEntryRepository.CreateAsync(_mapper.Map<AccountEntry>(message));
            }
        }
    }
}
