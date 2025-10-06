using FluxoDeCaixa.Caixa.Domain.Entities;
using FluxoDeCaixa.Caixa.Domain.Repositories;
using FluxoDeCaixa.Caixa.Models;
using FluxoDeCaixa.Caixa.Services;
using API.Core.Controllers;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace FluxoDeCaixa.Caixa.Controllers
{
    [Route("api/account")]
    public class AccountController : BaseController
    {
        private readonly AccountService _accountService;
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper;

        public AccountController(AccountService accountService,
            IAccountRepository accountRepository,
            IMapper mapper)
        {
            _accountService = accountService;
            _accountRepository = accountRepository;
            _mapper = mapper;
        }

        [HttpGet("show-info")]
        public async Task<ActionResult> ShowAccountInfo()
        {
            try
            {
                var account = await _accountRepository.GetAccount();

                return CustomResponse(_mapper.Map<AccountModel>(account));
            }
            catch (Exception ex)
            {
                Erros.Add(ex.Message);

                return CustomResponse();
            }
        }

        [HttpPost("add-entry")]        
        public async Task<ActionResult> AddNewEntry(EntryModel entryModel)
        {
            try
            {
                if (!ModelState.IsValid) return CustomResponse(ModelState);

                Guid userId = new Guid("499DDC45-4912-42B9-996F-101473FC2042"); //usuario fixo porque nao foi implementado a parte de autenticacao

                await _accountService.AddNewEntry(userId, _mapper.Map<Entry>(entryModel));

                return CustomResponse("Lançamento criado com sucesso");
            }
            catch (Exception ex)
            {
                Erros.Add(ex.Message);

                return CustomResponse();
            }
        }
    }
}
