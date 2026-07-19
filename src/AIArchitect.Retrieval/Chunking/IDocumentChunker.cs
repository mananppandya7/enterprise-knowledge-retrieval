using AIArchitect.Retrieval.Models;

namespace AIArchitect.Retrieval.Chunking;

/// <summary>
/// Converts one supported document type into meaningful retrieval units.
/// Different formats need different implementations because a contract clause, case-study
/// section, and spreadsheet row do not share the same useful semantic boundary.
/// </summary>
public interface IDocumentChunker
{
    DocumentType SupportedDocumentType { get; }

    IReadOnlyList<DocumentChunk> Chunk(SourceDocument document);
}

