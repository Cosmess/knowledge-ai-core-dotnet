using System.Reflection;
using Npgsql;

namespace KnowledgeAi.Infrastructure.Persistence;

/// <summary>
/// Applies the idempotent schema (Schema.sql, embedded resource) against the configured database.
/// Suitable for an MVP; a dedicated migration tool can replace this once the schema needs versioning.
/// </summary>
public sealed class DatabaseInitializer
{
    private readonly NpgsqlDataSource _dataSource;

    public DatabaseInitializer(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var schema = ReadEmbeddedSchema();

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(schema, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);

        // On a fresh database, the connection above bootstraps its type catalog before
        // "create extension vector" (part of the schema script) has run, so it caches "vector"
        // as unknown. Reload so later commands can resolve Pgvector.Vector parameters.
        await connection.ReloadTypesAsync();
    }

    private static string ReadEmbeddedSchema()
    {
        var assembly = Assembly.GetExecutingAssembly();
        const string resourceName = "KnowledgeAi.Infrastructure.Persistence.Migrations.Schema.sql";

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
