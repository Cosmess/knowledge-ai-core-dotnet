using KnowledgeAi.Application.Common.Interfaces;
using KnowledgeAi.Application.Common.Mediator;
using KnowledgeAi.Domain.Entities;

namespace KnowledgeAi.Application.Documents.Queries;

public sealed class ListDocumentsQueryHandler : IRequestHandler<ListDocumentsQuery, IReadOnlyList<Document>>
{
    private readonly IDocumentRepository _documentRepository;

    public ListDocumentsQueryHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public Task<IReadOnlyList<Document>> Handle(ListDocumentsQuery request, CancellationToken cancellationToken) =>
        _documentRepository.ListDocumentsAsync(cancellationToken);
}
