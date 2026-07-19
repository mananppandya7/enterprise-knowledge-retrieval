namespace AIArchitect.Retrieval.Models;

/// <summary>
/// A ranked semantic-search match and its similarity score.
/// </summary>
public sealed record SearchResult(
    int Rank,
    double SimilarityScore,
    DocumentChunk Chunk);
