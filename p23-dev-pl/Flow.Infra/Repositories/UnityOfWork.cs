using Flow.Core.Repositories;
using Flow.Infra.Data;

namespace Flow.Infra.Repositories
{
    public class UnityOfWork : IUnityOfWork
    {
        private readonly RelationalDbContext _context;
        private bool _disposed;
        public UnityOfWork(RelationalDbContext context)
        {
            _context = context;
        }

        public async Task Commit() =>
            await _context.SaveChangesAsync();

        public void Dispose()
        {
            Dispose(true);
        }

        public void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _context.Dispose();
            }

            _disposed = true;
        }
    }
}
