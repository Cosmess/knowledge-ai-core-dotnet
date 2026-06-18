using System.Security.Claims;
using KnowledgeAi.Application.Common.Interfaces;
using KnowledgeAi.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;

namespace KnowledgeAi.Api.Auth;

public sealed class HttpContextCurrentUserAccessor : ICurrentUserAccessor
{
    private readonly ClaimsPrincipal _user;

    public HttpContextCurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _user = httpContextAccessor.HttpContext?.User
            ?? throw new InvalidOperationException("No HttpContext available to resolve the current user.");
    }

    public Guid UserId =>
        Guid.Parse(_user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? _user.FindFirstValue("sub")
            ?? throw new InvalidOperationException("Current principal has no 'sub' claim."));

    public Role Role =>
        Enum.Parse<Role>(_user.FindFirstValue(ClaimTypes.Role)
            ?? throw new InvalidOperationException("Current principal has no role claim."));

    public IReadOnlySet<string> AllowedSpaceKeys =>
        _user.FindAll("space").Select(claim => claim.Value).ToHashSet();
}
