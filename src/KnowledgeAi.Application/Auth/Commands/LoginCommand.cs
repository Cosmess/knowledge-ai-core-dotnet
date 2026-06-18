using KnowledgeAi.Application.Common.Interfaces;
using KnowledgeAi.Application.Common.Mediator;

namespace KnowledgeAi.Application.Auth.Commands;

public sealed record LoginCommand(string Email, string Password) : IRequest<AccessToken>;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AccessToken>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public LoginCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtTokenGenerator tokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AccessToken> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        return _tokenGenerator.GenerateToken(user);
    }
}
