using AIArchitect.Retrieval.Chunking;
using AIArchitect.Retrieval.Documents;
using AIArchitect.Retrieval.Models;

namespace AIArchitect.Retrieval.Tests;

public sealed class DocumentChunkingTests
{
    [Fact]
    public void ChunkCorpus_AllDocumentsProduceUniqueChunksWithSourceMetadata()
    {
        IReadOnlyList<SourceDocument> documents = SampleDocumentFactory.CreateAll();
        DocumentChunkerFactory factory = CreateFactory();

        DocumentChunk[] chunks = documents
            .SelectMany(factory.ChunkDocument)
            .ToArray();

        Assert.Equal(40, chunks.Length);
        Assert.Equal(
            chunks.Length,
            chunks.Select(chunk => chunk.ChunkId).Distinct(StringComparer.OrdinalIgnoreCase).Count());

        foreach (DocumentChunk chunk in chunks)
        {
            SourceDocument source = Assert.Single(
                documents,
                document => document.Id == chunk.DocumentId);

            Assert.Equal(source.FileName, chunk.SourceFileName);
            Assert.Equal(source.DocumentType, chunk.DocumentType);
            Assert.All(source.Metadata, entry =>
                Assert.Equal(entry.Value, chunk.Metadata[entry.Key]));
        }
    }

    [Fact]
    public void SowChunker_KeepsHeadingAttachedWithoutMixingAdjacentSections()
    {
        SourceDocument document = Document(DocumentType.StatementOfWork);
        IReadOnlyList<DocumentChunk> chunks = new SowChunker().Chunk(document);

        DocumentChunk scope = Assert.Single(
            chunks,
            chunk => chunk.SectionTitle == "3. Scope");

        Assert.Contains("## 3. Scope", scope.Text);
        Assert.Contains("browser-based user interface", scope.Text);
        Assert.DoesNotContain("## 4. Modernization Approach", scope.Text);
    }

    [Fact]
    public void CaseStudyChunker_PreservesHealthcareSolutionAsIndependentSemanticUnit()
    {
        SourceDocument document = SampleDocumentFactory.CreateAll()
            .Single(item => item.Id == "healthcare-distributor-case-study");
        IReadOnlyList<DocumentChunk> chunks = new CaseStudyChunker().Chunk(document);

        DocumentChunk solution = Assert.Single(
            chunks,
            chunk => chunk.SectionTitle == "AI and Automation Solution");

        Assert.Contains(document.Title, solution.Text);
        Assert.Contains("Demand-forecasting models", solution.Text);
        Assert.DoesNotContain("## Outcomes", solution.Text);
    }

    [Fact]
    public void TestPlanChunker_PreservesStructuredTestFieldsAndIdentifiers()
    {
        SourceDocument document = Document(DocumentType.TestPlan);
        IReadOnlyList<DocumentChunk> chunks = new TestPlanChunker().Chunk(document);

        Assert.Equal("workbook-summary", chunks[0].Metadata["chunk-kind"]);
        DocumentChunk paymentCase = Assert.Single(
            chunks,
            chunk => chunk.Metadata.GetValueOrDefault("test-id") == "CHK-032");

        Assert.Equal("Payment", paymentCase.Metadata["module"]);
        Assert.Equal("P1", paymentCase.Metadata["priority"]);
        Assert.Contains("Test ID: CHK-032", paymentCase.Text);
        Assert.Contains("Expected Result:", paymentCase.Text);
    }

    [Fact]
    public void ProposalChunker_CreatesDistinctOperationalPhaseChunks()
    {
        SourceDocument document = Document(DocumentType.SalesProposal);
        IReadOnlyList<DocumentChunk> chunks = new ProposalChunker().Chunk(document);

        Assert.Equal(6, chunks.Count);
        DocumentChunk sales = Assert.Single(
            chunks,
            chunk => chunk.SectionTitle == "Sales and Quoting");

        Assert.Contains("pre-visit intake assistant", sales.Text);
        Assert.DoesNotContain("## Ordering and Procurement", sales.Text);
    }

    [Theory]
    [InlineData(DocumentType.StatementOfWork)]
    [InlineData(DocumentType.CaseStudy)]
    [InlineData(DocumentType.TestPlan)]
    [InlineData(DocumentType.SalesProposal)]
    public void Factory_ProducesContiguousSequences(DocumentType documentType)
    {
        SourceDocument document = Document(documentType);
        IReadOnlyList<DocumentChunk> chunks = CreateFactory().ChunkDocument(document);

        Assert.Equal(Enumerable.Range(0, chunks.Count), chunks.Select(chunk => chunk.Sequence));
    }

    private static SourceDocument Document(DocumentType documentType) =>
        SampleDocumentFactory.CreateAll().First(document => document.DocumentType == documentType);

    private static DocumentChunkerFactory CreateFactory() => new(
    [
        new SowChunker(),
        new CaseStudyChunker(),
        new TestPlanChunker(),
        new ProposalChunker()
    ]);
}
