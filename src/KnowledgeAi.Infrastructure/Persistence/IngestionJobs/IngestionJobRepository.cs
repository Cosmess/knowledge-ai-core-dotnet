using Dapper;
using KnowledgeAi.Application.Common.Interfaces;
using KnowledgeAi.Domain.Entities;
using KnowledgeAi.Domain.ValueObjects;
using Npgsql;

namespace KnowledgeAi.Infrastructure.Persistence.IngestionJobs;

public sealed class IngestionJobRepository : IIngestionJobRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public IngestionJobRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<IngestionJob> CreateAsync(IngestionJob job, CancellationToken cancellationToken)
    {
        const string sql = """
            insert into ingestion_jobs (id, source, space_key, status, documents_processed, chunks_processed, error, started_at, completed_at)
            values (@Id, @Source, @SpaceKey, @Status, @DocumentsProcessed, @ChunksProcessed, @Error, @StartedAt, @CompletedAt);
            """;

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(sql, ToParameters(job), cancellationToken: cancellationToken));
        return job;
    }

    public async Task UpdateAsync(IngestionJob job, CancellationToken cancellationToken)
    {
        const string sql = """
            update ingestion_jobs set
                status = @Status,
                documents_processed = @DocumentsProcessed,
                chunks_processed = @ChunksProcessed,
                error = @Error,
                completed_at = @CompletedAt
            where id = @Id;
            """;

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(sql, ToParameters(job), cancellationToken: cancellationToken));
    }

    private static object ToParameters(IngestionJob job) => new
    {
        job.Id,
        Source = job.Source.ToString(),
        job.SpaceKey,
        Status = job.Status.ToString(),
        job.DocumentsProcessed,
        job.ChunksProcessed,
        job.Error,
        StartedAt = job.StartedAt.UtcDateTime,
        CompletedAt = job.CompletedAt?.UtcDateTime,
    };
}
