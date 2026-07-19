using AIArchitect.Retrieval.Models;

namespace AIArchitect.Retrieval.Chunking;

/// <summary>
/// Shared mechanics for documents whose authored headings are meaningful retrieval boundaries.
/// Keeping a heading with its body protects clause or opportunity-area context. A bounded word
/// window is used only for unusually large sections, trading a little duplication for continuity.
/// </summary>
public abstract class HeadingAwareChunkerBase : DocumentChunkerBase
{
    private const int MaxWordsPerChunk = 600;
    private const int OverlapWords = 60;

    protected abstract string StrategyName { get; }

    public override IReadOnlyList<DocumentChunk> Chunk(SourceDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        IReadOnlyList<DocumentSection> sections = MarkdownSectionParser.Parse(document.Content);
        var chunks = new List<DocumentChunk>();
        int sequence = 0;

        foreach (DocumentSection section in sections)
        {
            IReadOnlyList<string> parts = WordWindowSplitter.Split(
                section.Body,
                MaxWordsPerChunk,
                OverlapWords);

            for (int partIndex = 0; partIndex < parts.Count; partIndex++)
            {
                string text = $"{document.Title}\n\n## {section.Title}\n{parts[partIndex]}";
                chunks.Add(CreateChunk(
                    document,
                    section.Title,
                    text,
                    sequence++,
                    Metadata(
                        ("chunking-strategy", StrategyName),
                        ("section-part", (partIndex + 1).ToString()),
                        ("section-part-count", parts.Count.ToString()))));
            }
        }

        return chunks;
    }

    private static IEnumerable<KeyValuePair<string, string>> Metadata(
        params (string Key, string Value)[] entries) =>
        entries.Select(entry => KeyValuePair.Create(entry.Key, entry.Value));
}

