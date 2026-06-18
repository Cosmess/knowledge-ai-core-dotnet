using KnowledgeAi.Application.Common.Mediator;
using KnowledgeAi.Domain.Entities;

namespace KnowledgeAi.Application.Documents.Queries;

public sealed record ListDocumentsQuery : IRequest<IReadOnlyList<Document>>;
