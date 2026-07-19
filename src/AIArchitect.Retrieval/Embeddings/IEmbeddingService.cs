namespace AIArchitect.Retrieval.Embeddings;

/// <summary>
/// Isolates the retrieval pipeline from a specific embedding provider and keeps tests free from
/// paid API calls. Implementations must preserve input order when embedding a collection.
/// </summary>
public interface IEmbeddingService
{
    string Model { get; }

    Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(
        string input,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(
        IReadOnlyCollection<string> inputs,
        CancellationToken cancellationToken = default);
}

