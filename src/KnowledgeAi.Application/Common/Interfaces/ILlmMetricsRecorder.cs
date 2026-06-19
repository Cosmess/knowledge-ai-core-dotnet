namespace KnowledgeAi.Application.Common.Interfaces;

public interface ILlmMetricsRecorder
{
    /// <summary>Records one chat completion attempt: token usage and estimated cost, or a fallback if the LLM call failed.</summary>
    void RecordCompletion(string provider, int? inputTokens, int? outputTokens, bool wasFallback);

    /// <summary>Records whether a chat/search request had enough retrieved evidence to answer.</summary>
    void RecordEvidenceOutcome(bool hasEnoughEvidence);
}
