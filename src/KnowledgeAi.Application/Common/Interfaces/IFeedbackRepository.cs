using FeedbackEntity = KnowledgeAi.Domain.Entities.Feedback;

namespace KnowledgeAi.Application.Common.Interfaces;

public interface IFeedbackRepository
{
    Task SaveAsync(FeedbackEntity feedback, CancellationToken cancellationToken);
}
