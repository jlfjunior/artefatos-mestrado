using CashFlow.Application.Common.Storage;

namespace CashFlow.Infrastructure.Common.Storage
{
    public class StorageService : IStorageService
    {
        public Task DeleteAsync(StorageFile storageFile)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> DownloadAsync(StorageFile storageFile, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task UploadAsync(UploadStorageInput input, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
