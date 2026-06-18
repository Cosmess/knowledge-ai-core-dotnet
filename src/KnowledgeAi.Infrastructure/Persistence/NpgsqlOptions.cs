namespace KnowledgeAi.Infrastructure.Persistence;

public sealed class NpgsqlOptions
{
    public const string SectionName = "Postgres";

    public required string ConnectionString { get; set; }
}
