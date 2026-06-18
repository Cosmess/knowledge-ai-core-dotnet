using KnowledgeAi.Application.Chat.Commands;
using KnowledgeAi.Application.Common.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgeAi.Api.Controllers;

[ApiController]
[Route("chat")]
[Authorize]
public sealed class ChatController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChatController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<AskQuestionResult>> Ask(AskQuestionCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
