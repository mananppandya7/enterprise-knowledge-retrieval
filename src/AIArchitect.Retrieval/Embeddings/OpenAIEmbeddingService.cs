using OpenAI.Embeddings;

namespace AIArchitect.Retrieval.Embeddings;

/// <summary>
/// Generates embeddings through the official OpenAI .NET SDK. Credentials remain outside source
/// control and are read from OPENAI_API_KEY; the optional OPENAI_EMBEDDING_MODEL setting supports
/// deliberate model changes without coupling the rest of the application to the provider.
/// </summary>
public sealed class OpenAIEmbeddingService : IEmbeddingService
{
    public const string DefaultModel = "text-embedding-3-small";

    private readonly EmbeddingClient _client;

    public OpenAIEmbeddingService(string apiKey, string model = DefaultModel)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(model);

        Model = model;
        _client = new EmbeddingClient(model, apiKey);
    }

    public string Model { get; }

    public static OpenAIEmbeddingService CreateFromEnvironment()
    {
        string? apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "OPENAI_API_KEY is not set. Configure it in the process environment before " +
                "running embedding generation; never commit the key to source control.");
        }

        string? configuredModel = Environment.GetEnvironmentVariable("OPENAI_EMBEDDING_MODEL");
        string model = string.IsNullOrWhiteSpace(configuredModel)
            ? DefaultModel
            : configuredModel;

        return new OpenAIEmbeddingService(apiKey, model);
    }

    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(
        string input,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        try
        {
            OpenAIEmbedding embedding = await _client.GenerateEmbeddingAsync(
                input,
                options: null,
                cancellationToken);

            return embedding.ToFloats();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            throw ProviderFailure(exception);
        }
    }

    public async Task<IReadOnlyList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(
        IReadOnlyCollection<string> inputs,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        if (inputs.Count == 0)
        {
            throw new ArgumentException("At least one input is required.", nameof(inputs));
        }

        if (inputs.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("Embedding inputs cannot be null, empty, or whitespace.", nameof(inputs));
        }

        // Materializing once makes the request order explicit. The SDK returns embeddings in the
        // corresponding input order, which lets the caller safely zip vectors back to chunks.
        string[] orderedInputs = inputs.ToArray();

        try
        {
            OpenAIEmbeddingCollection embeddings = await _client.GenerateEmbeddingsAsync(
                orderedInputs,
                options: null,
                cancellationToken);

            ReadOnlyMemory<float>[] vectors = embeddings
                .Select(embedding => embedding.ToFloats())
                .ToArray();

            if (vectors.Length != orderedInputs.Length)
            {
                throw new InvalidOperationException(
                    $"The embedding provider returned {vectors.Length} vectors for " +
                    $"{orderedInputs.Length} inputs.");
            }

            return vectors;
        }
        catch (Exception exception) when (exception is not OperationCanceledException and not EmbeddingServiceException)
        {
            throw ProviderFailure(exception);
        }
    }

    private EmbeddingServiceException ProviderFailure(Exception exception) =>
        new(
            $"OpenAI embedding generation failed for model '{Model}'. Verify API credentials, " +
            "model access, network connectivity, and request limits.",
            exception);
}
