using AIArchitect.Retrieval.Models;

namespace AIArchitect.Retrieval.Chunking;

/// <summary>
/// Chunks a statement of work by its numbered sections so scope, assumptions, exclusions,
/// and commercial clauses do not become detached from their headings.
/// </summary>
public sealed class SowChunker : HeadingAwareChunkerBase
{
    public override DocumentType SupportedDocumentType => DocumentType.StatementOfWork;

    protected override string StrategyName => "numbered-section-aware";
}

