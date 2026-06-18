using KnowledgeAi.Application.Common.Interfaces;
using KnowledgeAi.Application.Common.Mediator;

namespace KnowledgeAi.Application.Documents.Queries;

public sealed class ListSpacesQueryHandler : IRequestHandler<ListSpacesQuery, IReadOnlyList<string>>
{
    private readonly IDocumentRepository _documentRepository;

    public ListSpacesQueryHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public Task<IReadOnlyList<string>> Handle(ListSpacesQuery request, CancellationToken cancellationToken) =>
        _documentRepository.ListSpacesAsync(cancellationToken);
}
