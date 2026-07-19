using AIArchitect.Retrieval.Models;

namespace AIArchitect.Retrieval.Chunking;

/// <summary>
/// Chunks a proposal by opportunity area, keeping each business problem, proposed intervention,
/// and expected benefit in the same retrieval unit whenever the authored section permits it.
/// </summary>
public sealed class ProposalChunker : HeadingAwareChunkerBase
{
    public override DocumentType SupportedDocumentType => DocumentType.SalesProposal;

    protected override string StrategyName => "opportunity-area-heading-aware";
}

