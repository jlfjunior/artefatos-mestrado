using Flow.Core.Repositories;
using Moq;

namespace CommomTestsUtilities.Repositories
{
    public class UnityOfWorkBuilder
    {
        private readonly Mock<IUnityOfWork> _unityOfWork;

        public UnityOfWorkBuilder() => _unityOfWork = new Mock<IUnityOfWork>();

        public void Commit() => _unityOfWork.Setup(uow => uow.Commit()).Returns(Task.CompletedTask);

        public Mock<IUnityOfWork> Builder() => _unityOfWork;
    }
}
