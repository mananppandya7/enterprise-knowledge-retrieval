using AIArchitect.Retrieval.Models;

namespace AIArchitect.Retrieval.Output;

public static class ConsoleResultWriter
{
    public static void Write(string query, IReadOnlyList<SearchResult> results)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        ArgumentNullException.ThrowIfNull(results);

        Console.WriteLine();
        Console.WriteLine($"Query: {query}");
        Console.WriteLine();
        Console.WriteLine($"Top {results.Count} results");
        Console.WriteLine();

        if (results.Count == 0)
        {
            Console.WriteLine("No indexed chunks matched the query.");
            return;
        }

        foreach (SearchResult result in results)
        {
            Console.WriteLine($"Rank {result.Rank} | Score: {result.SimilarityScore:F4}");
            Console.WriteLine($"Source: {result.Chunk.SourceFileName}");
            Console.WriteLine($"Type: {DisplayName(result.Chunk.DocumentType)}");
            Console.WriteLine($"Section: {result.Chunk.SectionTitle}");
            Console.WriteLine($"Chunk: {result.Chunk.ChunkId}");
            Console.WriteLine("Text:");
            Console.WriteLine(result.Chunk.Text);
            Console.WriteLine();
            Console.WriteLine(new string('-', 60));
        }
    }

    private static string DisplayName(DocumentType documentType) => documentType switch
    {
        DocumentType.StatementOfWork => "Statement of Work",
        DocumentType.CaseStudy => "Case Study",
        DocumentType.TestPlan => "Test Plan",
        DocumentType.SalesProposal => "Sales Proposal",
        _ => documentType.ToString()
    };
}
