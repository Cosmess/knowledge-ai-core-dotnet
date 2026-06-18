using KnowledgeAi.Application.Common.Interfaces;
using KnowledgeAi.Application.Common.Mediator;
using KnowledgeAi.Domain.Entities;

namespace KnowledgeAi.Application.Documents.Queries;

public sealed record GetDocumentByIdQuery(Guid Id) : IRequest<Document?>;

public sealed class GetDocumentByIdQueryHandler : IRequestHandler<GetDocumentByIdQuery, Document?>
{
    private readonly IDocumentRepository _documentRepository;

    public GetDocumentByIdQueryHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public Task<Document?> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken) =>
        _documentRepository.GetDocumentByIdAsync(request.Id, cancellationToken);
}
