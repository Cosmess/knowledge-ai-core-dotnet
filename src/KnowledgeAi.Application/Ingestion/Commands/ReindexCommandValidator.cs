using FluentValidation;
using KnowledgeAi.Domain.ValueObjects;

namespace KnowledgeAi.Application.Ingestion.Commands;

public sealed class ReindexCommandValidator : AbstractValidator<ReindexCommand>
{
    public ReindexCommandValidator()
    {
        RuleFor(command => command.SpaceKey).NotEmpty();
        RuleFor(command => command.RootDir)
            .NotEmpty()
            .When(command => command.Source == DocumentSource.Markdown)
            .WithMessage("RootDir is required when reindexing from Markdown.");
    }
}
