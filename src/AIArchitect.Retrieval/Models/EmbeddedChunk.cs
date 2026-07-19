namespace AIArchitect.Retrieval.Models;

/// <summary>
/// Associates a document chunk with the vector produced by an embedding provider.
/// </summary>
public sealed record EmbeddedChunk(
    DocumentChunk Chunk,
    ReadOnlyMemory<float> Embedding);

