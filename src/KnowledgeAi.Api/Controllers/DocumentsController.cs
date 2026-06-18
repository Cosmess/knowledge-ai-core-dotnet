using KnowledgeAi.Application.Common.Mediator;
using KnowledgeAi.Application.Documents.Queries;
using KnowledgeAi.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgeAi.Api.Controllers;

[ApiController]
[Route("documents")]
[Authorize]
public sealed class DocumentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DocumentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Document>>> List(CancellationToken cancellationToken)
    {
        var documents = await _mediator.Send(new ListDocumentsQuery(), cancellationToken);
        return Ok(documents);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Document>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var document = await _mediator.Send(new GetDocumentByIdQuery(id), cancellationToken);
        return document is null ? NotFound() : Ok(document);
    }

    [HttpGet("/spaces")]
    public async Task<ActionResult<IReadOnlyList<string>>> ListSpaces(CancellationToken cancellationToken)
    {
        var spaces = await _mediator.Send(new ListSpacesQuery(), cancellationToken);
        return Ok(spaces);
    }
}
