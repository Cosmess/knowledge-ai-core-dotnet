using KnowledgeAi.Api.Auth;
using KnowledgeAi.Api.Contracts;
using KnowledgeAi.Application.Common.Mediator;
using KnowledgeAi.Application.Search.Queries;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgeAi.Api.Controllers;

[ApiController]
[Route("mcp")]
[Authorize(AuthenticationSchemes = ApiKeyDefaults.Scheme)]
public sealed class McpController : ControllerBase
{
    private readonly IMediator _mediator;

    public McpController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("search")]
    public async Task<ActionResult<McpSearchResult>> Search(McpSearchRequest request, CancellationToken cancellationToken)
    {
        // The API key above only proves the call came from a trusted MCP process; it carries no end-user
        // identity. We require a user JWT on top of it so AllowedSpaceKeys can be enforced per caller.
        var jwtResult = await HttpContext.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
        if (!jwtResult.Succeeded || jwtResult.Principal is null)
        {
            return Unauthorized();
        }

        HttpContext.User = jwtResult.Principal;

        var result = await _mediator.Send(request.ToQuery(), cancellationToken);
        return Ok(result);
    }
}
