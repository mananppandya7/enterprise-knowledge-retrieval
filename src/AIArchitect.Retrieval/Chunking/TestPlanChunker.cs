using System.Text;
using AIArchitect.Retrieval.Models;

namespace AIArchitect.Retrieval.Chunking;

/// <summary>
/// Treats the test plan as structured records rather than arbitrary text. A workbook summary
/// supports broad coverage questions, while one chunk per test case keeps identifiers, module,
/// scenario, expected result, and priority together for precise retrieval and traceability.
/// </summary>
public sealed class TestPlanChunker : DocumentChunkerBase
{
    public override DocumentType SupportedDocumentType => DocumentType.TestPlan;

    public override IReadOnlyList<DocumentChunk> Chunk(SourceDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        ParsedTestPlan parsed = Parse(document.Content);
        var chunks = new List<DocumentChunk>();
        int sequence = 0;

        chunks.Add(CreateChunk(
            document,
            sectionTitle: "Workbook Summary",
            text: $"{document.Title}\n\n## Workbook Summary\n{parsed.Summary}",
            sequence: sequence++,
            additionalMetadata: Metadata(
                ("chunking-strategy", "structured-test-record"),
                ("chunk-kind", "workbook-summary"))));

        foreach (TestCaseRecord testCase in parsed.TestCases)
        {
            string sectionTitle = $"{testCase.Sheet} / {testCase.Module} / {testCase.Id}";
            string text =
                $"{document.Title}\n\n" +
                $"Sheet: {testCase.Sheet}\n" +
                $"Test ID: {testCase.Id}\n" +
                $"Module: {testCase.Module}\n" +
                $"Scenario: {testCase.Scenario}\n" +
                $"Expected Result: {testCase.ExpectedResult}\n" +
                $"Priority: {testCase.Priority}";

            chunks.Add(CreateChunk(
                document,
                sectionTitle,
                text,
                sequence++,
                Metadata(
                    ("chunking-strategy", "structured-test-record"),
                    ("chunk-kind", "test-case"),
                    ("sheet", testCase.Sheet),
                    ("test-id", testCase.Id),
                    ("module", testCase.Module),
                    ("priority", testCase.Priority))));
        }

        return chunks;
    }

    private static ParsedTestPlan Parse(string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        var summary = new StringBuilder();
        var testCases = new List<TestCaseRecord>();
        string? currentSheet = null;

        foreach (string rawLine in content.ReplaceLineEndings("\n").Split('\n'))
        {
            string line = rawLine.Trim();

            if (line.StartsWith("## Sheet:", StringComparison.OrdinalIgnoreCase))
            {
                currentSheet = line["## Sheet:".Length..].Trim();
                continue;
            }

            if (string.IsNullOrWhiteSpace(line) ||
                line.Equals("# Workbook Summary", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (currentSheet is null)
            {
                summary.AppendLine(line);
                continue;
            }

            if (line.StartsWith("Test ID |", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string[] fields = line.Split('|', StringSplitOptions.TrimEntries);
            if (fields.Length == 5)
            {
                testCases.Add(new TestCaseRecord(
                    Sheet: currentSheet,
                    Id: fields[0],
                    Module: fields[1],
                    Scenario: fields[2],
                    ExpectedResult: fields[3],
                    Priority: fields[4]));
            }
        }

        if (string.IsNullOrWhiteSpace(summary.ToString()))
        {
            throw new InvalidOperationException("The test plan does not contain a workbook summary.");
        }

        if (testCases.Count == 0)
        {
            throw new InvalidOperationException("The test plan does not contain any structured test cases.");
        }

        return new ParsedTestPlan(summary.ToString().Trim(), testCases);
    }

    private static IEnumerable<KeyValuePair<string, string>> Metadata(
        params (string Key, string Value)[] entries) =>
        entries.Select(entry => KeyValuePair.Create(entry.Key, entry.Value));

    private sealed record ParsedTestPlan(
        string Summary,
        IReadOnlyList<TestCaseRecord> TestCases);

    private sealed record TestCaseRecord(
        string Sheet,
        string Id,
        string Module,
        string Scenario,
        string ExpectedResult,
        string Priority);
}

