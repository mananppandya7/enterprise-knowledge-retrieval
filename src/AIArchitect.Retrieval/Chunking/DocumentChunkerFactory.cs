using AIArchitect.Retrieval.Models;

namespace AIArchitect.Retrieval.Chunking;

/// <summary>
/// Resolves chunkers by document type and validates their output at the orchestration boundary.
/// This prevents callers from silently falling back to one fixed-size strategy for every format.
/// </summary>
public sealed class DocumentChunkerFactory
{
    private readonly IReadOnlyDictionary<DocumentType, IDocumentChunker> _chunkers;

    public DocumentChunkerFactory(IEnumerable<IDocumentChunker> chunkers)
    {
        ArgumentNullException.ThrowIfNull(chunkers);

        IDocumentChunker[] registrations = chunkers.ToArray();
        DocumentType[] duplicateTypes = registrations
            .GroupBy(chunker => chunker.SupportedDocumentType)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateTypes.Length > 0)
        {
            throw new ArgumentException(
                $"Multiple chunkers are registered for: {string.Join(", ", duplicateTypes)}.",
                nameof(chunkers));
        }

        _chunkers = registrations.ToDictionary(chunker => chunker.SupportedDocumentType);
    }

    public IDocumentChunker GetChunker(DocumentType documentType) =>
        _chunkers.TryGetValue(documentType, out IDocumentChunker? chunker)
            ? chunker
            : throw new NotSupportedException(
                $"No chunker is registered for document type '{documentType}'.");

    public IReadOnlyList<DocumentChunk> ChunkDocument(SourceDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        IReadOnlyList<DocumentChunk> chunks = GetChunker(document.DocumentType).Chunk(document);
        ValidateChunks(document, chunks);
        return chunks;
    }

    private static void ValidateChunks(
        SourceDocument document,
        IReadOnlyList<DocumentChunk>? chunks)
    {
        if (chunks is null || chunks.Count == 0)
        {
            throw new InvalidOperationException(
                $"Chunker produced no chunks for '{document.FileName}'.");
        }

        var chunkIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int index = 0; index < chunks.Count; index++)
        {
            DocumentChunk chunk = chunks[index]
                ?? throw new InvalidOperationException(
                    $"Chunker returned a null chunk at position {index} for '{document.FileName}'.");

            if (string.IsNullOrWhiteSpace(chunk.ChunkId) || !chunkIds.Add(chunk.ChunkId))
            {
                throw new InvalidOperationException(
                    $"Chunk IDs must be non-empty and unique within '{document.FileName}'.");
            }

            if (chunk.DocumentId != document.Id ||
                chunk.SourceFileName != document.FileName ||
                chunk.DocumentType != document.DocumentType)
            {
                throw new InvalidOperationException(
                    $"Chunk '{chunk.ChunkId}' does not preserve its source document identity.");
            }

            if (chunk.Sequence != index)
            {
                throw new InvalidOperationException(
                    $"Chunk sequences for '{document.FileName}' must be contiguous and zero-based.");
            }

            if (string.IsNullOrWhiteSpace(chunk.SectionTitle) ||
                string.IsNullOrWhiteSpace(chunk.Text))
            {
                throw new InvalidOperationException(
                    $"Chunk '{chunk.ChunkId}' must contain a section title and text.");
            }

            bool sourceMetadataPreserved = document.Metadata.All(entry =>
                chunk.Metadata.TryGetValue(entry.Key, out string? value) && value == entry.Value);

            if (!sourceMetadataPreserved)
            {
                throw new InvalidOperationException(
                    $"Chunk '{chunk.ChunkId}' does not preserve its source metadata.");
            }
        }
    }
}
