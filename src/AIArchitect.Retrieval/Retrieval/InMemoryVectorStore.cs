using AIArchitect.Retrieval.Models;

namespace AIArchitect.Retrieval.Retrieval;

/// <summary>
/// A deliberately small local vector store for the assessment corpus. Search is an exhaustive
/// cosine scan, which is transparent and adequate for tens of chunks but not intended to replace
/// an approximate-nearest-neighbor index at production scale.
/// </summary>
public sealed class InMemoryVectorStore : IVectorStore
{
    private readonly object _syncRoot = new();
    private readonly List<EmbeddedChunk> _items = [];
    private readonly HashSet<string> _chunkIds = new(StringComparer.OrdinalIgnoreCase);
    private int? _vectorDimension;

    public int Count
    {
        get
        {
            lock (_syncRoot)
            {
                return _items.Count;
            }
        }
    }

    public void Add(EmbeddedChunk embeddedChunk)
    {
        ArgumentNullException.ThrowIfNull(embeddedChunk);
        AddRange([embeddedChunk]);
    }

    public void AddRange(IEnumerable<EmbeddedChunk> embeddedChunks)
    {
        ArgumentNullException.ThrowIfNull(embeddedChunks);
        EmbeddedChunk[] candidates = embeddedChunks.ToArray();

        if (candidates.Length == 0)
        {
            return;
        }

        lock (_syncRoot)
        {
            int expectedDimension = _vectorDimension ?? candidates[0].Embedding.Length;
            var candidateIds = new HashSet<string>(_chunkIds, StringComparer.OrdinalIgnoreCase);

            // Validate the complete batch before mutating the store so a bad item cannot leave a
            // partially indexed corpus.
            foreach (EmbeddedChunk candidate in candidates)
            {
                ArgumentNullException.ThrowIfNull(candidate);

                if (string.IsNullOrWhiteSpace(candidate.Chunk.ChunkId) ||
                    !candidateIds.Add(candidate.Chunk.ChunkId))
                {
                    throw new InvalidOperationException(
                        $"Chunk ID '{candidate.Chunk.ChunkId}' is empty or already indexed.");
                }

                ValidateVector(candidate.Embedding.Span, expectedDimension, "embedding");
            }

            foreach (EmbeddedChunk candidate in candidates)
            {
                // Own the vector memory so caller mutation cannot silently corrupt the index.
                var stored = new EmbeddedChunk(candidate.Chunk, candidate.Embedding.ToArray());
                _items.Add(stored);
                _chunkIds.Add(stored.Chunk.ChunkId);
            }

            _vectorDimension = expectedDimension;
        }
    }

    public IReadOnlyList<SearchResult> Search(ReadOnlyMemory<float> queryEmbedding, int topK)
    {
        if (topK <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(topK), "Top K must be greater than zero.");
        }

        EmbeddedChunk[] snapshot;
        int? expectedDimension;

        lock (_syncRoot)
        {
            snapshot = [.. _items];
            expectedDimension = _vectorDimension;
        }

        if (snapshot.Length == 0)
        {
            return [];
        }

        ValidateVector(queryEmbedding.Span, expectedDimension!.Value, nameof(queryEmbedding));

        return snapshot
            .Select(item => new
            {
                Item = item,
                Score = CosineSimilarity.Calculate(queryEmbedding.Span, item.Embedding.Span)
            })
            .OrderByDescending(match => match.Score)
            .ThenBy(match => match.Item.Chunk.ChunkId, StringComparer.Ordinal)
            .Take(Math.Min(topK, snapshot.Length))
            .Select((match, index) => new SearchResult(
                Rank: index + 1,
                SimilarityScore: match.Score,
                Chunk: match.Item.Chunk))
            .ToArray();
    }

    private static void ValidateVector(
        ReadOnlySpan<float> vector,
        int expectedDimension,
        string parameterName)
    {
        if (vector.IsEmpty)
        {
            throw new ArgumentException("Embedding vectors cannot be empty.", parameterName);
        }

        if (vector.Length != expectedDimension)
        {
            throw new ArgumentException(
                $"Embedding dimension {vector.Length} does not match the store dimension " +
                $"{expectedDimension}.",
                parameterName);
        }

        bool hasNonZeroValue = false;
        foreach (float value in vector)
        {
            if (!float.IsFinite(value))
            {
                throw new ArgumentException(
                    "Embedding vectors must contain only finite values.",
                    parameterName);
            }

            hasNonZeroValue |= value != 0;
        }

        if (!hasNonZeroValue)
        {
            throw new ArgumentException(
                "Embedding vectors must have non-zero magnitude.",
                parameterName);
        }
    }
}

