using FluentValidation;

namespace KnowledgeAi.Application.Chat.Commands;

public sealed class AskQuestionCommandValidator : AbstractValidator<AskQuestionCommand>
{
    public AskQuestionCommandValidator()
    {
        RuleFor(command => command.Question).NotEmpty().MaximumLength(2000);
        RuleFor(command => command.SpaceKey).NotEmpty();
        RuleFor(command => command.System).NotEmpty();
    }
}
