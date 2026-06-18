using FluentAssertions;
using KnowledgeAi.Application.Auth.Commands;

namespace KnowledgeAi.UnitTests.Application.Auth.Commands;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenEmailAndPasswordAreValid_Passes()
    {
        var result = _validator.Validate(new LoginCommand("user@example.com", "secret"));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "secret")]
    [InlineData("not-an-email", "secret")]
    [InlineData("user@example.com", "")]
    public void Validate_WhenEmailOrPasswordAreInvalid_Fails(string email, string password)
    {
        var result = _validator.Validate(new LoginCommand(email, password));

        result.IsValid.Should().BeFalse();
    }
}
