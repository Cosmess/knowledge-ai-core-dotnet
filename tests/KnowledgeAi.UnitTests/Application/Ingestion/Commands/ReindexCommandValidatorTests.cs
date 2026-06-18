using FluentAssertions;
using KnowledgeAi.Application.Ingestion.Commands;
using KnowledgeAi.Domain.ValueObjects;

namespace KnowledgeAi.UnitTests.Application.Ingestion.Commands;

public class ReindexCommandValidatorTests
{
    private readonly ReindexCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenSourceIsMarkdownAndRootDirIsMissing_Fails()
    {
        var result = _validator.Validate(new ReindexCommand(DocumentSource.Markdown, "ENG", RootDir: null));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WhenSourceIsMarkdownAndRootDirIsPresent_Passes()
    {
        var result = _validator.Validate(new ReindexCommand(DocumentSource.Markdown, "ENG", RootDir: "docs/"));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenSourceIsConfluenceAndRootDirIsMissing_Passes()
    {
        var result = _validator.Validate(new ReindexCommand(DocumentSource.Confluence, "ENG", RootDir: null));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenSpaceKeyIsEmpty_Fails()
    {
        var result = _validator.Validate(new ReindexCommand(DocumentSource.Confluence, "", RootDir: null));

        result.IsValid.Should().BeFalse();
    }
}
