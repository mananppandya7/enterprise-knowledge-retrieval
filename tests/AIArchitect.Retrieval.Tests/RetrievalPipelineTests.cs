using AIArchitect.Retrieval.Chunking;
using AIArchitect.Retrieval.Documents;
using AIArchitect.Retrieval.Embeddings;
using AIArchitect.Retrieval.Models;
using AIArchitect.Retrieval.Retrieval;
using AIArchitect.Retrieval.Tests.TestDoubles;

namespace AIArchitect.Retrieval.Tests;

public sealed class RetrievalPipelineTests
{
    private const string Query = "AI solutions delivered for healthcare clients";

    [Fact]
    public async Task Search_WithFakeEmbeddings_RanksExpectedChunksWithoutApiCall()
    {
        IReadOnlyList<SourceDocument> documents = SampleDocumentFactory.CreateAll();
        var chunkerFactory = new DocumentChunkerFactory(
        [
            new SowChunker(),
            new CaseStudyChunker(),
            new TestPlanChunker(),
            new ProposalChunker()
        ]);
        DocumentChunk[] chunks = documents.SelectMany(chunkerFactory.ChunkDocument).ToArray();
        IEmbeddingService embeddings = new FakeEmbeddingService(VectorFor);

        IReadOnlyList<ReadOnlyMemory<float>> vectors =
            await embeddings.GenerateEmbeddingsAsync(chunks.Select(chunk => chunk.Text).ToArray());

        IVectorStore store = new InMemoryVectorStore();
        store.AddRange(chunks.Select((chunk, index) => new EmbeddedChunk(chunk, vectors[index])));
        ReadOnlyMemory<float> queryVector = await embeddings.GenerateEmbeddingAsync(Query);

        IReadOnlyList<SearchResult> results = store.Search(queryVector, topK: 3);

        Assert.Collection(
            results,
            first =>
            {
                Assert.Equal("AI and Automation Solution", first.Chunk.SectionTitle);
                Assert.Equal("CaseStudy_HealthcareDistributor.pdf", first.Chunk.SourceFileName);
                Assert.Equal("healthcare distribution", first.Chunk.Metadata["industry"]);
            },
            second => Assert.Equal("Outcomes", second.Chunk.SectionTitle),
            third =>
            {
                Assert.Equal("Executive Summary", third.Chunk.SectionTitle);
                Assert.Equal(DocumentType.SalesProposal, third.Chunk.DocumentType);
            });

        Assert.True(results[0].SimilarityScore > results[1].SimilarityScore);
        Assert.True(results[1].SimilarityScore > results[2].SimilarityScore);
    }

    private static float[] VectorFor(string input)
    {
        if (input == Query || input.Contains("## AI and Automation Solution", StringComparison.Ordinal))
        {
            return [1f, 0f];
        }

        if (input.Contains("Healthcare Distributor", StringComparison.Ordinal) &&
            input.Contains("## Outcomes", StringComparison.Ordinal))
        {
            return [0.95f, 0.05f];
        }

        if (input.Contains("AI Transformation Proposal", StringComparison.Ordinal) &&
            input.Contains("## Executive Summary", StringComparison.Ordinal))
        {
            return [0.8f, 0.2f];
        }

        return [0f, 1f];
    }
}
