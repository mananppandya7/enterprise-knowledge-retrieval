namespace AIArchitect.Retrieval.Models;

/// <summary>
/// Identifies the document structure so a later stage can select an appropriate chunking strategy.
/// </summary>
public enum DocumentType
{
    StatementOfWork,
    CaseStudy,
    TestPlan,
    SalesProposal
}

