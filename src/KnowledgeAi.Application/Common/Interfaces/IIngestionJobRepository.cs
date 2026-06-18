using KnowledgeAi.Domain.Entities;

namespace KnowledgeAi.Application.Common.Interfaces;

public interface IIngestionJobRepository
{
    Task<IngestionJob> CreateAsync(IngestionJob job, CancellationToken cancellationToken);

    Task UpdateAsync(IngestionJob job, CancellationToken cancellationToken);
}
