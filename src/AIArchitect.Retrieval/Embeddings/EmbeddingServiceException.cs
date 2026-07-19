namespace AIArchitect.Retrieval.Embeddings;

/// <summary>
/// Adds actionable application context to an embedding-provider failure without exposing inputs
/// or credentials.
/// </summary>
public sealed class EmbeddingServiceException : Exception
{
    public EmbeddingServiceException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

