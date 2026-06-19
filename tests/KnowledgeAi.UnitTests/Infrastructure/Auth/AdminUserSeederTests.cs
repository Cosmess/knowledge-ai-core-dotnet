using FluentAssertions;
using KnowledgeAi.Application.Common.Interfaces;
using KnowledgeAi.Domain.Entities;
using KnowledgeAi.Domain.ValueObjects;
using KnowledgeAi.Infrastructure.Auth;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace KnowledgeAi.UnitTests.Infrastructure.Auth;

public class AdminUserSeederTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();

    [Fact]
    public async Task SeedAsync_WhenEmailAndPasswordAreConfigured_CreatesAdminWithHashedPassword()
    {
        var options = Options.Create(new AdminSeedOptions { Email = "admin@example.com", Password = "secret", SpaceKeys = ["ENG"] });
        _passwordHasher.Hash("secret").Returns("hashed-secret");
        var seeder = new AdminUserSeeder(_userRepository, _passwordHasher, options);

        await seeder.SeedAsync();

        await _userRepository.Received(1).CreateIfNotExistsAsync(
            Arg.Is<User>(u =>
                u.Email == "admin@example.com" &&
                u.PasswordHash == "hashed-secret" &&
                u.Role == Role.Admin &&
                u.AllowedSpaceKeys.Contains("ENG")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SeedAsync_WhenEmailIsNotConfigured_DoesNothing()
    {
        var options = Options.Create(new AdminSeedOptions { Email = "", Password = "secret" });
        var seeder = new AdminUserSeeder(_userRepository, _passwordHasher, options);

        await seeder.SeedAsync();

        await _userRepository.DidNotReceive().CreateIfNotExistsAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SeedAsync_WhenPasswordIsNotConfigured_DoesNothing()
    {
        var options = Options.Create(new AdminSeedOptions { Email = "admin@example.com", Password = "" });
        var seeder = new AdminUserSeeder(_userRepository, _passwordHasher, options);

        await seeder.SeedAsync();

        await _userRepository.DidNotReceive().CreateIfNotExistsAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }
}
