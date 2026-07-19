using AIArchitect.Retrieval.Models;

namespace AIArchitect.Retrieval.Chunking;

/// <summary>
/// Preserves the narrative units reviewers and users commonly search in a case study: client
/// profile, challenge, delivered solution, technical approach, and outcomes. The shared heading-
/// aware implementation prefixes each chunk with the document and section titles, so industry
/// context is retained even when a solution or outcome chunk is embedded independently.
/// </summary>
public sealed class CaseStudyChunker : HeadingAwareChunkerBase
{
    public override DocumentType SupportedDocumentType => DocumentType.CaseStudy;

    protected override string StrategyName => "case-study-semantic-section";
}

