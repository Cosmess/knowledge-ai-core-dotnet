using KnowledgeAi.Application.Common.Mediator;

namespace KnowledgeAi.Application.Documents.Queries;

public sealed record ListSpacesQuery : IRequest<IReadOnlyList<string>>;
