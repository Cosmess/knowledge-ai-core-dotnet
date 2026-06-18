using FluentValidation;

namespace KnowledgeAi.Application.Feedback.Commands;

public sealed class RecordFeedbackCommandValidator : AbstractValidator<RecordFeedbackCommand>
{
    public RecordFeedbackCommandValidator()
    {
        RuleFor(command => command.ChatMessageId).NotEqual(Guid.Empty);
        RuleFor(command => command.Comment).MaximumLength(1000);
    }
}
