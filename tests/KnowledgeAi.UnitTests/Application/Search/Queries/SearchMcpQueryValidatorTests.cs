using FluentAssertions;
using KnowledgeAi.Application.Search.Queries;

namespace KnowledgeAi.UnitTests.Application.Search.Queries;

public class SearchMcpQueryValidatorTests
{
    private readonly SearchMcpQueryValidator _validator = new();

    [Fact]
    public void Validate_WhenQueryAndLimitAreValid_Passes()
    {
        var result = _validator.Validate(new SearchMcpQuery("how does auth work", null, null, null, null, Limit: 5));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenQueryIsEmpty_Fails()
    {
        var result = _validator.Validate(new SearchMcpQuery("", null, null, null, null, Limit: 5));

        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public void Validate_WhenLimitIsOutOfRange_Fails(int limit)
    {
        var result = _validator.Validate(new SearchMcpQuery("query", null, null, null, null, limit));

        result.IsValid.Should().BeFalse();
    }
}
