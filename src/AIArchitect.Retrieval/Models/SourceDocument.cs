using System.Collections.Immutable;

namespace AIArchitect.Retrieval.Models;

/// <summary>
/// A source document before it has been divided into retrieval chunks.
/// </summary>
public sealed record SourceDocument(
    string Id,
    string FileName,
    DocumentType DocumentType,
    string Title,
    string Content,
    ImmutableDictionary<string, string> Metadata)
{
    public static SourceDocument Create(
        string id,
        string fileName,
        DocumentType documentType,
        string title,
        string content,
        IEnumerable<KeyValuePair<string, string>>? metadata = null) =>
        new(
            id,
            fileName,
            documentType,
            title,
            content,
            metadata?.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase)
                ?? ImmutableDictionary<string, string>.Empty.WithComparers(StringComparer.OrdinalIgnoreCase));
}

