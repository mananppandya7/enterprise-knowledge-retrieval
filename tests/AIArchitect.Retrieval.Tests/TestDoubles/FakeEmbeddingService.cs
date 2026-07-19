using AIArchitect.Retrieval.Embeddings;

namespace AIArchitect.Retrieval.Tests.TestDoubles;

internal sealed class FakeEmbeddingService(Func<string, float[]> vectorFactory) : IEmbeddingService
{
    public string Model => "deterministic-test-embedding";

    public Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(
        string input,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrWhiteSpace(input);
        return Task.FromResult<ReadOnlyMemory<float>>(vectorFactory(input).ToArray());
    }

    public Task<IReadOnlyList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(
        IReadOnlyCollection<string> inputs,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(inputs);

        IReadOnlyList<ReadOnlyMemory<float>> vectors = inputs
            .Select(input => (ReadOnlyMemory<float>)vectorFactory(input).ToArray())
            .ToArray();

        return Task.FromResult(vectors);
    }
}

