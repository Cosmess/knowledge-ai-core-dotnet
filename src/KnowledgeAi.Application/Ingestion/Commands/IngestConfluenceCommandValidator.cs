using FluentValidation;

namespace KnowledgeAi.Application.Ingestion.Commands;

public sealed class IngestConfluenceCommandValidator : AbstractValidator<IngestConfluenceCommand>
{
    public IngestConfluenceCommandValidator()
    {
        RuleFor(command => command.SpaceKey).NotEmpty();
    }
}
