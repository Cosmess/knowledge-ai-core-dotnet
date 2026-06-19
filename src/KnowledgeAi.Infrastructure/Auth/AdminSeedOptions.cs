namespace KnowledgeAi.Infrastructure.Auth;

public sealed class AdminSeedOptions
{
    public const string SectionName = "AdminSeed";

    public required string Email { get; set; }

    public required string Password { get; set; }

    public string[] SpaceKeys { get; set; } = [];
}
