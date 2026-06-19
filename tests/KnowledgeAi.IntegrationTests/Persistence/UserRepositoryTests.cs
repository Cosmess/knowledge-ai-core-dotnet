using FluentAssertions;
using KnowledgeAi.Domain.Entities;
using KnowledgeAi.Domain.ValueObjects;
using KnowledgeAi.Infrastructure.Persistence;
using KnowledgeAi.Infrastructure.Persistence.Users;
using Npgsql;
using Testcontainers.PostgreSql;

namespace KnowledgeAi.IntegrationTests.Persistence;

public sealed class UserRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("pgvector/pgvector:pg16")
        .WithDatabase("knowledgeai")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private NpgsqlDataSource _dataSource = null!;
    private UserRepository _repository = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        _dataSource = NpgsqlDataSourceFactory.Create(_container.GetConnectionString());
        await new DatabaseInitializer(_dataSource).InitializeAsync();

        _repository = new UserRepository(_dataSource);
    }

    public async Task DisposeAsync()
    {
        await _dataSource.DisposeAsync();
        await _container.DisposeAsync();
    }

    [Fact]
    public async Task CreateIfNotExistsAsync_WhenEmailIsNew_CreatesUserAndReturnsTrue()
    {
        var user = BuildUser("admin@example.com");

        var created = await _repository.CreateIfNotExistsAsync(user, CancellationToken.None);

        created.Should().BeTrue();
        var stored = await _repository.GetByEmailAsync("admin@example.com", CancellationToken.None);
        stored.Should().NotBeNull();
        stored!.Role.Should().Be(Role.Admin);
        stored.AllowedSpaceKeys.Should().Contain("ENG");
    }

    [Fact]
    public async Task CreateIfNotExistsAsync_WhenEmailAlreadyExists_ReturnsFalseAndKeepsOriginal()
    {
        var first = BuildUser("duplicate@example.com");
        await _repository.CreateIfNotExistsAsync(first, CancellationToken.None);

        var second = BuildUser("duplicate@example.com");
        var created = await _repository.CreateIfNotExistsAsync(second, CancellationToken.None);

        created.Should().BeFalse();
    }

    private static User BuildUser(string email) => new()
    {
        Id = Guid.NewGuid(),
        Email = email,
        PasswordHash = "hashed",
        Role = Role.Admin,
        AllowedSpaceKeys = new HashSet<string> { "ENG" },
        CreatedAt = DateTimeOffset.UtcNow,
    };
}
