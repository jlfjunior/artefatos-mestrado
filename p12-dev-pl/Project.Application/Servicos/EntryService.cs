using AutoMapper;
using Project.Application.Interfaces;
using Project.Application.Utils;
using Project.Application.ViewModels;
using Project.Domain;
using Project.Domain.Entities;

namespace Project.Application.Servicos
{
    public class EntryService : IEntryService
    {
        private readonly IMapper _mapper;
        private readonly IEntryRepository _entryRepository;
        private readonly ILogsService _logsService;

        public EntryService(IMapper mapper, IEntryRepository entryRepository, ILogsService logsService)
        {
            _mapper = mapper;
            _entryRepository = entryRepository;
            _logsService = logsService;
        }

        public async Task<CustomResult<EntryVM>> AddEntry(string email, EntryVM entryVM)
        {
            try
            {
                await _logsService.Add(email, "EntryService", "AddEntry", string.Empty);

                entryVM.DateEntry = DateTime.Now;

                var entryMap = _mapper.Map<Entry>(entryVM);

                var result = await _entryRepository.Add(entryMap);
                var resultMap = _mapper.Map<EntryVM>(result);

                return CustomResult<EntryVM>.Success(resultMap);
            }
            catch (Exception ex)
            {
                await _logsService.Add(email, "EntryService", "AddEntry", ex.Message);

                return CustomResult<EntryVM>.Failure(CustomError.ExceptionError(ex.Message));
            }
        }

        public async Task<CustomResult<EntryVM>> UpdateEntry(string email, EntryVM entryVM)
        {
            try
            {
                await _logsService.Add(email, "EntryService", "UpdateEntry", string.Empty);

                entryVM.DateEntry = DateTime.Now;

                var entryMap = _mapper.Map<Entry>(entryVM);

                var result = await _entryRepository.Update(entryMap);
                var resultMap = _mapper.Map<EntryVM>(result);

                return CustomResult<EntryVM>.Success(resultMap);
            }
            catch (Exception ex)
            {
                await _logsService.Add(email, "EntryService", "UpdateEntry", ex.Message);

                return CustomResult<EntryVM>.Failure(CustomError.ExceptionError(ex.Message));
            }
        }

        public async Task<CustomResult<EntryVM>> GetItem(string email, int id)
        {
            try
            {
                await _logsService.Add(email, "EntryService", "GetItem", string.Empty);

                var resultList = await _entryRepository.GetItem(id);
                if (resultList == null)
                {
                    return CustomResult<EntryVM>.Failure(CustomError.RecordNotFound("Lançamento não encontrado"));
                }

                var result = _mapper.Map<EntryVM>(resultList);

                return CustomResult<EntryVM>.Success(result);
            }
            catch (Exception ex)
            {
                await _logsService.Add(email, "EntryService", "GetItem", ex.Message);

                return CustomResult<EntryVM>.Failure(CustomError.ExceptionError(ex.Message));
            }
        }

        public async Task<CustomResult<IEnumerable<EntryVM>>> GetAll(string email)
        {
            try
            {
                await _logsService.Add(email, "EntryService", "GetAll", string.Empty);

                var resultList = await _entryRepository.GetAll();
                var result = _mapper.Map<IEnumerable<EntryVM>>(resultList);

                return CustomResult<IEnumerable<EntryVM>>.Success(result);
            }
            catch (Exception ex)
            {
                await _logsService.Add(email, "EntryService", "GetAll", ex.Message);

                return CustomResult<IEnumerable<EntryVM>>.Failure(CustomError.ExceptionError(ex.Message));
            }
        }

        public async Task<CustomResult<int>> DeleteEntry(string email, int id)
        {
            try
            {
                await _logsService.Add(email, "EntryService", "DeleteEntry", string.Empty);

                var result = await _entryRepository.Delete(id);

                return CustomResult<int>.Success(result);
            }
            catch (Exception ex)
            {
                await _logsService.Add(email, "EntryService", "DeleteEntry", ex.Message);

                return CustomResult<int>.Failure(CustomError.ExceptionError(ex.Message));
            }
        }
    }
}
