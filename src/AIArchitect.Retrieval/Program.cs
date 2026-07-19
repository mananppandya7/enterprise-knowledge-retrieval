using AIArchitect.Retrieval.Chunking;
using AIArchitect.Retrieval.Documents;
using AIArchitect.Retrieval.Embeddings;
using AIArchitect.Retrieval.Models;
using AIArchitect.Retrieval.Output;
using AIArchitect.Retrieval.Retrieval;

const string query = "AI solutions delivered for healthcare clients";
const int topK = 3;

using var cancellationSource = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationSource.Cancel();
};

try
{
    IReadOnlyList<SourceDocument> documents = SampleDocumentFactory.CreateAll();
    var chunkerFactory = new DocumentChunkerFactory(
    [
        new SowChunker(),
        new CaseStudyChunker(),
        new TestPlanChunker(),
        new ProposalChunker()
    ]);

    var chunks = new List<DocumentChunk>();

    Console.WriteLine("Ingestion summary");
    Console.WriteLine(new string('-', 60));

    foreach (SourceDocument document in documents)
    {
        IReadOnlyList<DocumentChunk> documentChunks = chunkerFactory.ChunkDocument(document);
        chunks.AddRange(documentChunks);
        Console.WriteLine(
            $"{document.FileName} | {document.DocumentType} | {documentChunks.Count} chunks");
    }

    Console.WriteLine(new string('-', 60));
    Console.WriteLine($"Total: {documents.Count} documents | {chunks.Count} chunks");
    Console.WriteLine();

    IEmbeddingService embeddingService = OpenAIEmbeddingService.CreateFromEnvironment();
    Console.WriteLine($"Generating embeddings with {embeddingService.Model}...");

    IReadOnlyList<ReadOnlyMemory<float>> vectors =
        await embeddingService.GenerateEmbeddingsAsync(
            chunks.Select(chunk => chunk.Text).ToArray(),
            cancellationSource.Token);

    IVectorStore vectorStore = new InMemoryVectorStore();
    vectorStore.AddRange(chunks.Select((chunk, index) =>
        new EmbeddedChunk(chunk, vectors[index])));

    ReadOnlyMemory<float> queryEmbedding =
        await embeddingService.GenerateEmbeddingAsync(query, cancellationSource.Token);

    IReadOnlyList<SearchResult> results = vectorStore.Search(queryEmbedding, topK);
    ConsoleResultWriter.Write(query, results);
}
catch (OperationCanceledException)
{
    Console.Error.WriteLine("Operation cancelled.");
    Environment.ExitCode = 130;
}
catch (Exception exception)
{
    // Keep command-line failures concise and actionable. Provider exceptions are sanitized by
    // OpenAIEmbeddingService so credentials and full input content are never printed.
    Console.Error.WriteLine($"Error: {exception.Message}");
    Environment.ExitCode = 1;
}

