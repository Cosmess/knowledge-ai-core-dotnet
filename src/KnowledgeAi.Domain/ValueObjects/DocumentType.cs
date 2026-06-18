namespace KnowledgeAi.Domain.ValueObjects;

/// <summary>
/// Mirrors the "Tipos De Documento" taxonomy from docs/data/metadata-taxonomy.md.
/// </summary>
public enum DocumentType
{
    TechnicalDoc,
    BusinessRule,
    ApiDoc,
    ArchitectureDecision,
    UserStory,
    NextTask,
    Runbook,
    Faq,
    OnboardingDoc,
    ProductDoc,
    OperationalProcess,
    EventContract,
    DatabaseDoc,
    IntegrationDoc
}
