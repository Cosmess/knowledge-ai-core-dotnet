using FluentValidation;

namespace KnowledgeAi.Application.Ingestion.Commands;

public sealed class IngestMarkdownCommandValidator : AbstractValidator<IngestMarkdownCommand>
{
    public IngestMarkdownCommandValidator()
    {
        RuleFor(command => command.RootDir).NotEmpty();
        RuleFor(command => command.SpaceKey).NotEmpty();
    }
}
