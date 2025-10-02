using CashFlowControl.Core.Application.DTOs;
using CashFlowControl.Core.Application.Security.Helpers;
using MediatR;

namespace CashFlowControl.Core.Application.Commands
{
    public class ValidateTokenCommand : IRequest<Result<UserTokenValidationDto>>
    {
        public String Token { get; set; }
        public string Scheme { get; set; }
        public ValidateTokenCommand(string token, string scheme)
        {
            Token = token;
            Scheme = scheme;
        }
    }
}
