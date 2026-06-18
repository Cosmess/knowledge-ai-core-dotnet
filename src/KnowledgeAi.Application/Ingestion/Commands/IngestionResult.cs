namespace KnowledgeAi.Application.Ingestion.Commands;

public sealed record IngestionResult(Guid JobId, int DocumentsProcessed, int ChunksProcessed);
