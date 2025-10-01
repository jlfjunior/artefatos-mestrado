using CashFlow.Entries.Domain.Entities;
using CashFlow.Entries.Domain.Enums;
using CashFlow.Entries.Domain.Exceptions;
using CashFlow.Entries.Domain.Interfaces;

namespace CashFlow.Entries.Application.Service
{
    public class EntrieService : IEntryService
    {
        private readonly IEntryRepository _entrieRepository;

        public EntrieService(IEntryRepository entrieRepository)
        {
            _entrieRepository = entrieRepository ?? throw new ArgumentNullException(nameof(entrieRepository));
        }

        public async Task<Entry> CreateAsync(decimal value, string description, EntryType type, DateTime? date)
        {
            var entrie = new Entry(date ?? DateTime.Now, value, description, type);
            if (entrie.Errors.Any()) 
            {
                throw new EntityValidationFailException(entrie.Errors);
            }

            try
            {
                var entrieCreated = await _entrieRepository.CreateAsync(entrie);

                return entrieCreated;
            }
            catch (Exception)
            {
                throw new Exception("Failed to create entry.");
            }
        }
    }
}
