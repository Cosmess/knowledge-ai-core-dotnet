using KnowledgeAi.Application.Common.Mediator;
using KnowledgeAi.Domain.ValueObjects;

namespace KnowledgeAi.Application.Ingestion.Commands;

public sealed class ReindexCommandHandler : IRequestHandler<ReindexCommand, IngestionResult>
{
    private readonly IMediator _mediator;

    public ReindexCommandHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task<IngestionResult> Handle(ReindexCommand request, CancellationToken cancellationToken)
    {
        return request.Source switch
        {
            DocumentSource.Markdown => _mediator.Send(
                new IngestMarkdownCommand(request.RootDir ?? "docs", request.SpaceKey), cancellationToken),
            DocumentSource.Confluence => _mediator.Send(
                new IngestConfluenceCommand(request.SpaceKey), cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(request), request.Source, "Unsupported ingestion source.")
        };
    }
}
