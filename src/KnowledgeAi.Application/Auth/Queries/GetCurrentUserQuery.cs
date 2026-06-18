using KnowledgeAi.Application.Common.Interfaces;
using KnowledgeAi.Application.Common.Mediator;
using KnowledgeAi.Domain.Entities;

namespace KnowledgeAi.Application.Auth.Queries;

public sealed record GetCurrentUserQuery : IRequest<User>;

public sealed class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, User>
{
    private readonly ICurrentUserAccessor _currentUser;
    private readonly IUserRepository _userRepository;

    public GetCurrentUserQueryHandler(ICurrentUserAccessor currentUser, IUserRepository userRepository)
    {
        _currentUser = currentUser;
        _userRepository = userRepository;
    }

    public async Task<User> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        return await _userRepository.GetByIdAsync(_currentUser.UserId, cancellationToken)
            ?? throw new InvalidOperationException("Current user not found.");
    }
}
