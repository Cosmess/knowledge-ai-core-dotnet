using KnowledgeAi.Application.Common.Mediator;

namespace KnowledgeAi.Application.Ingestion.Commands;

public sealed record IngestMarkdownCommand(string RootDir, string SpaceKey) : IRequest<IngestionResult>;
