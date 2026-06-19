using KnowledgeAi.Application.Common.Interfaces;
using KnowledgeAi.Domain.Entities;
using KnowledgeAi.Domain.ValueObjects;
using Microsoft.Extensions.Options;

namespace KnowledgeAi.Infrastructure.Auth;

/// <summary>
/// Creates the configured admin user on startup if no user with that email exists yet.
/// Without this, a fresh database has no row in "users" and nobody can log in.
/// </summary>
public sealed class AdminUserSeeder
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly AdminSeedOptions _options;

    public AdminUserSeeder(IUserRepository userRepository, IPasswordHasher passwordHasher, IOptions<AdminSeedOptions> options)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _options = options.Value;
    }

    public Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.Email) || string.IsNullOrWhiteSpace(_options.Password))
        {
            return Task.CompletedTask;
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = _options.Email,
            PasswordHash = _passwordHasher.Hash(_options.Password),
            Role = Role.Admin,
            AllowedSpaceKeys = _options.SpaceKeys.ToHashSet(),
            CreatedAt = DateTimeOffset.UtcNow,
        };

        return _userRepository.CreateIfNotExistsAsync(user, cancellationToken);
    }
}
