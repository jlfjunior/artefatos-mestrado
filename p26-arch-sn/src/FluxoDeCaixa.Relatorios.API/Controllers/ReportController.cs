using API.Core.Controllers;
using AutoMapper;
using FluxoDeCaixa.Relatorios.API.Models;
using FluxoDeCaixa.Relatorios.API.NoSQLDatabase.Collections;
using FluxoDeCaixa.Relatorios.API.NoSQLDatabase.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FluxoDeCaixa.Relatorios.API.Controllers
{
    [Route("api/Report")]
    public class ReportController : BaseController
    {
        private readonly IAccountEntryRepository _accountEntryRepository;
        private readonly IMapper _mapper;

        public ReportController(IAccountEntryRepository accountEntryRepository, IMapper mapper)
        {
            _accountEntryRepository = accountEntryRepository;
            _mapper = mapper;
        }

        [HttpGet("show-month-report")]
        public async Task<ActionResult> ShowMonthReport(int month, int year)
        {
            try
            {
                DateTime initialDate = new DateTime(year, month, 1);
                DateTime endDate = new DateTime(year, month + 1, 1);

                var reportCollection = await _accountEntryRepository.FindAsync(x => x.EntryDate > initialDate && x.EntryDate < endDate);                

                return CustomResponse(GetReportModel(reportCollection));
            }
            catch (Exception ex)
            {
                Erros.Add(ex.Message);

                return CustomResponse();
            }
        }

        [HttpGet("show-daily-report")]
        public async Task<ActionResult> ShowDailyReport(int day, int month, int year)
        {
            try
            {
                DateTime initialDate = new DateTime(year, month, day);
                DateTime endDate = new DateTime(year, month, day).AddDays(1);

                var reportCollection = await _accountEntryRepository.FindAsync(x => x.EntryDate > initialDate && x.EntryDate < endDate);

                return CustomResponse(GetReportModel(reportCollection));
            }
            catch (Exception ex)
            {
                Erros.Add(ex.Message);

                return CustomResponse();
            }
        }

        private List<AccountEntryModel> GetReportModel(IEnumerable<AccountEntry> entries)
        {
            List<AccountEntryModel> accountEntries = new List<AccountEntryModel>();

            foreach (var entry in entries)
                accountEntries.Add(_mapper.Map<AccountEntryModel>(entry));

            return accountEntries.OrderBy(x => x.EntryDate).ToList();
        }
    }
}
