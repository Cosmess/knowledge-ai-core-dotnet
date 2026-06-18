using KnowledgeAi.Application.Common.Mediator;
using KnowledgeAi.Application.Ingestion.Commands;
using KnowledgeAi.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgeAi.Api.Controllers;

[ApiController]
[Authorize(Roles = nameof(Role.Admin))]
public sealed class IngestionController : ControllerBase
{
    private readonly IMediator _mediator;

    public IngestionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("ingest/markdown")]
    public async Task<ActionResult<IngestionResult>> IngestMarkdown(IngestMarkdownCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("ingest/confluence")]
    public async Task<ActionResult<IngestionResult>> IngestConfluence(IngestConfluenceCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("reindex")]
    public async Task<ActionResult<IngestionResult>> Reindex(ReindexCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
