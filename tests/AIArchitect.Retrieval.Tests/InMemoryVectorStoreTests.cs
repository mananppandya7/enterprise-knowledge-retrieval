using AIArchitect.Retrieval.Models;
using AIArchitect.Retrieval.Retrieval;

namespace AIArchitect.Retrieval.Tests;

public sealed class InMemoryVectorStoreTests
{
    [Fact]
    public void Add_DuplicateChunkId_ThrowsWithoutChangingStore()
    {
        var store = new InMemoryVectorStore();
        store.Add(Embedded("chunk-a", [1f, 0f]));

        Assert.Throws<InvalidOperationException>(
            () => store.Add(Embedded("CHUNK-A", [0f, 1f])));
        Assert.Equal(1, store.Count);
    }

    [Fact]
    public void AddRange_InvalidDimension_DoesNotPartiallyAddBatch()
    {
        var store = new InMemoryVectorStore();

        Assert.Throws<ArgumentException>(() => store.AddRange(
        [
            Embedded("chunk-a", [1f, 0f]),
            Embedded("chunk-b", [1f, 0f, 0f])
        ]));

        Assert.Equal(0, store.Count);
    }

    [Fact]
    public void Search_RanksDescendingAndPreservesChunkMetadata()
    {
        var store = new InMemoryVectorStore();
        store.AddRange(
        [
            Embedded("unrelated", [0f, 1f], section: "Other"),
            Embedded("closest", [1f, 0f], section: "AI Solution"),
            Embedded("related", [0.8f, 0.2f], section: "Outcomes")
        ]);

        IReadOnlyList<SearchResult> results = store.Search(new float[] { 1f, 0f }, topK: 2);

        Assert.Collection(
            results,
            first =>
            {
                Assert.Equal(1, first.Rank);
                Assert.Equal("closest", first.Chunk.ChunkId);
                Assert.Equal("AI Solution", first.Chunk.SectionTitle);
                Assert.Equal("source.pdf", first.Chunk.SourceFileName);
            },
            second =>
            {
                Assert.Equal(2, second.Rank);
                Assert.Equal("related", second.Chunk.ChunkId);
            });
    }

    [Fact]
    public void Search_EqualScores_UsesChunkIdAsDeterministicTieBreaker()
    {
        var store = new InMemoryVectorStore();
        store.AddRange(
        [
            Embedded("chunk-b", [1f, 0f]),
            Embedded("chunk-a", [1f, 0f])
        ]);

        IReadOnlyList<SearchResult> results = store.Search(new float[] { 1f, 0f }, topK: 2);

        Assert.Equal(["chunk-a", "chunk-b"], results.Select(result => result.Chunk.ChunkId));
    }

    [Fact]
    public void Search_TopKExceedsCount_ReturnsAllAvailableItems()
    {
        var store = new InMemoryVectorStore();
        store.Add(Embedded("only", [1f, 0f]));

        IReadOnlyList<SearchResult> results = store.Search(new float[] { 1f, 0f }, topK: 3);

        Assert.Single(results);
    }

    [Fact]
    public void Add_CopiesEmbeddingMemoryOwnedByCaller()
    {
        float[] vector = [1f, 0f];
        var store = new InMemoryVectorStore();
        store.Add(Embedded("chunk", vector));

        vector[0] = 0f;
        vector[1] = 1f;

        SearchResult result = Assert.Single(store.Search(new float[] { 1f, 0f }, topK: 1));
        Assert.Equal(1d, result.SimilarityScore, precision: 12);
    }

    private static EmbeddedChunk Embedded(
        string chunkId,
        float[] embedding,
        string section = "Section")
    {
        DocumentChunk chunk = DocumentChunk.Create(
            chunkId,
            documentId: "document",
            sourceFileName: "source.pdf",
            documentType: DocumentType.CaseStudy,
            sectionTitle: section,
            text: "Sample retrieval text",
            sequence: 0);

        return new EmbeddedChunk(chunk, embedding);
    }
}
