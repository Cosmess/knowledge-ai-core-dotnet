using FluentAssertions;
using KnowledgeAi.Infrastructure.Ingestion;

namespace KnowledgeAi.UnitTests.Infrastructure.Ingestion;

public class HeadingChunkingServiceTests
{
    private readonly HeadingChunkingService _service = new();

    [Fact]
    public void Split_TracksNestedHeadingPathAcrossSections()
    {
        var content =
            "# Title\n" +
            string.Join(" ", Enumerable.Repeat("intro", 35)) + "\n" +
            "## Subtitle\n" +
            string.Join(" ", Enumerable.Repeat("subtitle", 35));

        var chunks = _service.Split(content);

        chunks.Should().HaveCount(2);
        chunks[0].HeadingPath.Should().Be("Title");
        chunks[1].HeadingPath.Should().Be("Title > Subtitle");
    }

    [Fact]
    public void Split_MergesSectionsSmallerThanMinWordThreshold()
    {
        var content =
            "# Title\n" +
            string.Join(" ", Enumerable.Repeat("word", 40)) + "\n" +
            "## Tiny\n" +
            "just five short words here";

        var chunks = _service.Split(content);

        chunks.Should().HaveCount(1);
        chunks[0].Content.Should().Contain("just five short words here");
    }

    [Fact]
    public void Split_SplitsSectionsLargerThanWordBudgetIntoMultipleChunks()
    {
        var content = "# Title\n" + string.Join(" ", Enumerable.Repeat("word", 900));

        var chunks = _service.Split(content);

        chunks.Should().HaveCount(2);
        chunks.Should().OnlyContain(chunk => chunk.HeadingPath == "Title");
    }
}
