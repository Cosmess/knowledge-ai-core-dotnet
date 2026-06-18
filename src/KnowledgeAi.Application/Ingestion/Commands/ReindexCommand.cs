using KnowledgeAi.Application.Common.Mediator;
using KnowledgeAi.Domain.ValueObjects;

namespace KnowledgeAi.Application.Ingestion.Commands;

public sealed record ReindexCommand(DocumentSource Source, string SpaceKey, string? RootDir = null) : IRequest<IngestionResult>;
