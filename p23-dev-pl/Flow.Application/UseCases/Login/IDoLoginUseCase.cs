using Flow.Core.DTOs;

namespace Flow.Application.UseCases.Login
{
    public interface IDoLoginUseCase
    {
        Task<string> Execute(DoLoginRequest request);
    }
}
