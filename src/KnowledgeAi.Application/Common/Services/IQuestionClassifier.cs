using KnowledgeAi.Domain.ValueObjects;

namespace KnowledgeAi.Application.Common.Services;

public interface IQuestionClassifier
{
    KnowledgeDomain Classify(string question);
}
