using Dapper;
using KnowledgeAi.Application.Common.Interfaces;
using KnowledgeAi.Domain.Entities;
using KnowledgeAi.Domain.ValueObjects;
using Npgsql;

namespace KnowledgeAi.Infrastructure.Persistence.Users;

public sealed class UserRepository : IUserRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public UserRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        const string sql = """
            select id, email, password_hash, role, allowed_space_keys, created_at
            from users
            where email = @email;
            """;

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<UserRow>(new CommandDefinition(sql, new { email }, cancellationToken: cancellationToken));
        return row?.ToEntity();
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = """
            select id, email, password_hash, role, allowed_space_keys, created_at
            from users
            where id = @id;
            """;

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<UserRow>(new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken));
        return row?.ToEntity();
    }

    private sealed class UserRow
    {
        public Guid Id { get; init; }
        public required string Email { get; init; }
        public required string PasswordHash { get; init; }
        public required string Role { get; init; }
        public required string[] AllowedSpaceKeys { get; init; }
        public DateTime CreatedAt { get; init; }

        public User ToEntity() => new()
        {
            Id = Id,
            Email = Email,
            PasswordHash = PasswordHash,
            Role = Enum.Parse<Role>(Role, ignoreCase: true),
            AllowedSpaceKeys = AllowedSpaceKeys.ToHashSet(),
            CreatedAt = new DateTimeOffset(CreatedAt, TimeSpan.Zero),
        };
    }
}
