namespace KnowledgeAi.Domain.ValueObjects;

/// <summary>
/// Mirrors the "Domínios" taxonomy from docs/data/metadata-taxonomy.md.
/// Named KnowledgeDomain (not Domain) to avoid colliding with the KnowledgeAi.Domain project/namespace.
/// </summary>
public enum KnowledgeDomain
{
    Technical,
    Business,
    Operations,
    Product,
    Onboarding,
    Architecture,
    Api,
    Incident,
    Backlog,
    Runbook,
    Troubleshooting
}
