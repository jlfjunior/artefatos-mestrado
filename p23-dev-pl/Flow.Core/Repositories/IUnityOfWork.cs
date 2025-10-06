namespace Flow.Core.Repositories
{
    public interface IUnityOfWork
    {
        Task Commit();
    }
}
