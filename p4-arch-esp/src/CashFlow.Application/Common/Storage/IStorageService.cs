namespace CashFlow.Application.Common.Storage
{
    public interface IStorageService
    {
        Task UploadAsync(UploadStorageInput input, CancellationToken cancellationToken);
        Task<byte[]> DownloadAsync(StorageFile storageFile, CancellationToken cancellationToken);
        Task DeleteAsync(StorageFile storageFile);
    }
}
