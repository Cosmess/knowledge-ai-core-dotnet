using Dapper;
using Npgsql;
using Pgvector.Dapper;

namespace KnowledgeAi.Infrastructure.Persistence;

public static class NpgsqlDataSourceFactory
{
    public static NpgsqlDataSource Create(string connectionString)
    {
        // Repositories select snake_case columns (e.g. space_key, document_type) straight into
        // PascalCase row properties without aliasing; Dapper only bridges that gap when this is set.
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        SqlMapper.AddTypeHandler(new VectorTypeHandler());

        var builder = new NpgsqlDataSourceBuilder(connectionString);
        builder.UseVector();
        return builder.Build();
    }
}
