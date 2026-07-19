using System.Collections.Immutable;
using AIArchitect.Retrieval.Models;

namespace AIArchitect.Retrieval.Chunking;

/// <summary>
/// Supplies consistent chunk construction while leaving boundary detection to each document type.
/// </summary>
public abstract class DocumentChunkerBase : IDocumentChunker
{
    public abstract DocumentType SupportedDocumentType { get; }

    public abstract IReadOnlyList<DocumentChunk> Chunk(SourceDocument document);

    protected DocumentChunk CreateChunk(
        SourceDocument document,
        string sectionTitle,
        string text,
        int sequence,
        IEnumerable<KeyValuePair<string, string>>? additionalMetadata = null)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (document.DocumentType != SupportedDocumentType)
        {
            throw new ArgumentException(
                $"{GetType().Name} supports {SupportedDocumentType}, not {document.DocumentType}.",
                nameof(document));
        }

        ImmutableDictionary<string, string>.Builder metadata = document.Metadata.ToBuilder();
        metadata["document-title"] = document.Title;
        metadata["section"] = sectionTitle;

        if (additionalMetadata is not null)
        {
            foreach ((string key, string value) in additionalMetadata)
            {
                metadata[key] = value;
            }
        }

        return DocumentChunk.Create(
            chunkId: $"{document.Id}-{sequence:D3}",
            documentId: document.Id,
            sourceFileName: document.FileName,
            documentType: document.DocumentType,
            sectionTitle: sectionTitle,
            text: text,
            sequence: sequence,
            metadata: metadata);
    }
}

