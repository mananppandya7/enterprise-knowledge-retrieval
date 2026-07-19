using AIArchitect.Retrieval.Models;

namespace AIArchitect.Retrieval.Retrieval;

public interface IVectorStore
{
    int Count { get; }

    void Add(EmbeddedChunk embeddedChunk);

    void AddRange(IEnumerable<EmbeddedChunk> embeddedChunks);

    IReadOnlyList<SearchResult> Search(ReadOnlyMemory<float> queryEmbedding, int topK);
}

