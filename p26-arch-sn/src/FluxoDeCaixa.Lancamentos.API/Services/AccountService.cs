using FluxoDeCaixa.Caixa.Domain.Entities;
using FluxoDeCaixa.Caixa.Domain.Exceptions;
using FluxoDeCaixa.Caixa.Domain.Repositories;
using Core.Bus;
using Core.Bus.Messages;
using System;
using System.Threading.Tasks;

namespace FluxoDeCaixa.Caixa.Services
{
    public class AccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMessageBus _bus;

        public AccountService(IAccountRepository accountRepository,
            IUserRepository userRepository,
            IMessageBus bus)
        {
            _accountRepository = accountRepository;
            _userRepository = userRepository;
            _bus = bus;
        }

        public async Task<bool> AddNewEntry(Guid userId, Entry entry)
        {
            try
            {
                Account account = await _accountRepository.GetAccount();

                entry.UserId = userId;
                entry.AccountId = account.Id;

                account.UpdateValue(entry);

                _accountRepository.Update(account);
                _accountRepository.AddEntry(entry);

                await SendEntryToConsultDB(userId, account, entry);

                return await _accountRepository.UnitOfWork.Commit();

            }
            catch (InvalidCreditException)
            {
                throw new Exception("Valor de crédito inválido");
            }
            catch (InvalidDebitException)
            {
                throw new Exception("Valor de débito inválido");
            }
            catch (InsuficientFoundsException)
            {
                throw new Exception("Saldo insuficiente para realizar esta operação");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task SendEntryToConsultDB(Guid userId, Account account, Entry entry)
        {
            var user = await _userRepository.GetUserById(userId);

            AccountEntryMessage message = new AccountEntryMessage(
                account.Id,
                user.Name,
                (int)entry.Type,
                entry.Value,
                account.Value,
                entry.Description,
                DateTime.Now);

            if (!message.IsValid())
                throw new Exception("Ocorreu um erro ao salvar a operação");

            _ = _bus.PublishAsync(message);
        }
    }
}
