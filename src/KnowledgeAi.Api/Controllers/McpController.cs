using KnowledgeAi.Api.Auth;
using KnowledgeAi.Api.Contracts;
using KnowledgeAi.Application.Common.Mediator;
using KnowledgeAi.Application.Search.Queries;
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
        var result = await _mediator.Send(request.ToQuery(), cancellationToken);
        return Ok(result);
    }
}
