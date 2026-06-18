using FluentValidation;

namespace KnowledgeAi.Application.Search.Queries;

public sealed class SearchMcpQueryValidator : AbstractValidator<SearchMcpQuery>
{
    public SearchMcpQueryValidator()
    {
        RuleFor(query => query.Query).NotEmpty().MaximumLength(2000);
        RuleFor(query => query.Limit).InclusiveBetween(1, 50);
    }
}
