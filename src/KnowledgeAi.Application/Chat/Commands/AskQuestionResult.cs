using KnowledgeAi.Application.Common.Models;
using KnowledgeAi.Domain.ValueObjects;

namespace KnowledgeAi.Application.Chat.Commands;

public sealed record AskQuestionResult(
    string Answer,
    KnowledgeDomain Domain,
    IReadOnlyList<SourceReference> Sources,
    EvidenceStatus EvidenceStatus,
    double Confidence);
