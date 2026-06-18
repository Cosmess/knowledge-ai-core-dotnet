using KnowledgeAi.Application.Common.Mediator;
using KnowledgeAi.Domain.ValueObjects;

namespace KnowledgeAi.Application.Chat.Commands;

public sealed record AskQuestionCommand(
    string Question,
    Audience Audience,
    string SpaceKey,
    string System,
    Guid? ChatSessionId = null) : IRequest<AskQuestionResult>;
