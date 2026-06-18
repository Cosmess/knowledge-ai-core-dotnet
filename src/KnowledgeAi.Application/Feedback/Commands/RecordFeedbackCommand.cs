using KnowledgeAi.Application.Common.Mediator;

namespace KnowledgeAi.Application.Feedback.Commands;

public sealed record RecordFeedbackCommand(Guid ChatMessageId, bool Helpful, string? Comment) : IRequest<Unit>;
