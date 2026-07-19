using System.Collections.Immutable;

namespace AIArchitect.Retrieval.Models;

/// <summary>
/// A semantically meaningful, independently retrievable portion of a source document.
/// </summary>
public sealed record DocumentChunk(
    string ChunkId,
    string DocumentId,
    string SourceFileName,
    DocumentType DocumentType,
    string SectionTitle,
    string Text,
    int Sequence,
    ImmutableDictionary<string, string> Metadata)
{
    public static DocumentChunk Create(
        string chunkId,
        string documentId,
        string sourceFileName,
        DocumentType documentType,
        string sectionTitle,
        string text,
        int sequence,
        IEnumerable<KeyValuePair<string, string>>? metadata = null) =>
        new(
            chunkId,
            documentId,
            sourceFileName,
            documentType,
            sectionTitle,
            text,
            sequence,
            metadata?.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase)
                ?? ImmutableDictionary<string, string>.Empty.WithComparers(StringComparer.OrdinalIgnoreCase));
}

