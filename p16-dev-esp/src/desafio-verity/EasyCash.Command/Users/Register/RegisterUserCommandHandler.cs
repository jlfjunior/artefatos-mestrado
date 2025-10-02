using EasyCash.Abstractions.Authentication;
using EasyCash.Abstractions.Messaging.Commands;
using EasyCash.Domain.Users.Entities;
using EasyCash.Domain.Users.Interfaces;
using EasyCash.Domain.Users.ValueObjets;

namespace EasyCash.Command.Users.Register;

internal sealed class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, RegisterUserCommandResult>
{
    private readonly IRegisterService<UserEntity> _registerService;
    private readonly IUserRepository _userRepository;

    public RegisterUserCommandHandler(
        IRegisterService<UserEntity> registerService,
        IUserRepository userRepository)
    {
        _registerService = registerService;
        _userRepository = userRepository;
    }

    public async Task<RegisterUserCommandResult> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = UserEntity.Create(
            new FirstName(request.FirstName),
            new LastName(request.LastName),
            new Email(request.Email));

        var result = await _registerService.RegisterAsync(
            user,
            request.Password,
            cancellationToken);

        user.SetIdentityId(result);

        await _userRepository.AddAsync(user, cancellationToken);

        return new RegisterUserCommandResult(user.Id);
    }
}
