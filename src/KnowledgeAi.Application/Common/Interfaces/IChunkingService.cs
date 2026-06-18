namespace KnowledgeAi.Application.Common.Interfaces;

public sealed record TextChunk(string Content, string? HeadingPath);

public interface IChunkingService
{
    IReadOnlyList<TextChunk> Split(string content);
}
