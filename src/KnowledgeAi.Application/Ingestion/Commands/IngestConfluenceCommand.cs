using KnowledgeAi.Application.Common.Mediator;

namespace KnowledgeAi.Application.Ingestion.Commands;

public sealed record IngestConfluenceCommand(string SpaceKey) : IRequest<IngestionResult>;
