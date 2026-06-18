using KnowledgeAi.Api.Contracts;
using KnowledgeAi.Application.Auth.Commands;
using KnowledgeAi.Application.Auth.Queries;
using KnowledgeAi.Application.Common.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgeAi.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login(LoginCommand command, CancellationToken cancellationToken)
    {
        var token = await _mediator.Send(command, cancellationToken);
        return Ok(LoginResponse.FromAccessToken(token));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> Me(CancellationToken cancellationToken)
    {
        var user = await _mediator.Send(new GetCurrentUserQuery(), cancellationToken);
        return Ok(UserResponse.FromUser(user));
    }
}
