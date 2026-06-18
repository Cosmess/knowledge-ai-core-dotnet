using KnowledgeAi.Application.Common.Interfaces;
using KnowledgeAi.Application.Common.Mediator;
using FeedbackEntity = KnowledgeAi.Domain.Entities.Feedback;

namespace KnowledgeAi.Application.Feedback.Commands;

public sealed class RecordFeedbackCommandHandler : IRequestHandler<RecordFeedbackCommand, Unit>
{
    private readonly IFeedbackRepository _feedbackRepository;
    private readonly ICurrentUserAccessor _currentUser;

    public RecordFeedbackCommandHandler(IFeedbackRepository feedbackRepository, ICurrentUserAccessor currentUser)
    {
        _feedbackRepository = feedbackRepository;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(RecordFeedbackCommand request, CancellationToken cancellationToken)
    {
        await _feedbackRepository.SaveAsync(new FeedbackEntity
        {
            Id = Guid.NewGuid(),
            ChatMessageId = request.ChatMessageId,
            UserId = _currentUser.UserId,
            Helpful = request.Helpful,
            Comment = request.Comment,
            CreatedAt = DateTimeOffset.UtcNow
        }, cancellationToken);

        return Unit.Value;
    }
}
