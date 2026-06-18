using FluentAssertions;
using KnowledgeAi.Application.Chat.Commands;
using KnowledgeAi.Domain.ValueObjects;

namespace KnowledgeAi.UnitTests.Application.Chat.Commands;

public class AskQuestionCommandValidatorTests
{
    private readonly AskQuestionCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenCommandIsWellFormed_Passes()
    {
        var result = _validator.Validate(new AskQuestionCommand("How do I deploy?", Audience.Developers, "ENG", "knowledge-ai"));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenQuestionIsEmpty_Fails()
    {
        var result = _validator.Validate(new AskQuestionCommand("", Audience.Developers, "ENG", "knowledge-ai"));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WhenQuestionExceedsMaxLength_Fails()
    {
        var result = _validator.Validate(new AskQuestionCommand(new string('a', 2001), Audience.Developers, "ENG", "knowledge-ai"));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WhenSpaceKeyIsEmpty_Fails()
    {
        var result = _validator.Validate(new AskQuestionCommand("How do I deploy?", Audience.Developers, "", "knowledge-ai"));

        result.IsValid.Should().BeFalse();
    }
}
