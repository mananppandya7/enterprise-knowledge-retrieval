using System.Text;

namespace AIArchitect.Retrieval.Chunking;

internal sealed record DocumentSection(string Title, string Body);

internal static class MarkdownSectionParser
{
    public static IReadOnlyList<DocumentSection> Parse(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return [];
        }

        var sections = new List<DocumentSection>();
        var body = new StringBuilder();
        string? currentTitle = null;

        foreach (string rawLine in content.ReplaceLineEndings("\n").Split('\n'))
        {
            string line = rawLine.TrimEnd();

            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                AddSection(sections, currentTitle, body);
                currentTitle = line[3..].Trim();
                continue;
            }

            // The document title is added to every chunk, so it is not treated as a section body.
            if (currentTitle is null && line.StartsWith("# ", StringComparison.Ordinal))
            {
                continue;
            }

            if (currentTitle is not null || !string.IsNullOrWhiteSpace(line))
            {
                body.AppendLine(line);
            }
        }

        AddSection(sections, currentTitle, body);

        if (sections.Count == 0 && !string.IsNullOrWhiteSpace(content))
        {
            sections.Add(new DocumentSection("Overview", content.Trim()));
        }

        return sections;
    }

    private static void AddSection(
        ICollection<DocumentSection> sections,
        string? title,
        StringBuilder body)
    {
        string sectionBody = body.ToString().Trim();

        if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(sectionBody))
        {
            sections.Add(new DocumentSection(title, sectionBody));
        }

        body.Clear();
    }
}

