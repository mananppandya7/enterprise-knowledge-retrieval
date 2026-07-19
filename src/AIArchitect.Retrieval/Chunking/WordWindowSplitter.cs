namespace AIArchitect.Retrieval.Chunking;

internal static class WordWindowSplitter
{
    public static IReadOnlyList<string> Split(string text, int maxWords, int overlapWords)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        if (maxWords <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxWords), "Maximum words must be positive.");
        }

        if (overlapWords < 0 || overlapWords >= maxWords)
        {
            throw new ArgumentOutOfRangeException(
                nameof(overlapWords),
                "Overlap must be non-negative and smaller than the maximum chunk size.");
        }

        string[] words = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length <= maxWords)
        {
            return [text.Trim()];
        }

        var parts = new List<string>();
        int step = maxWords - overlapWords;

        for (int start = 0; start < words.Length; start += step)
        {
            int length = Math.Min(maxWords, words.Length - start);
            parts.Add(string.Join(' ', words, start, length));

            if (start + length >= words.Length)
            {
                break;
            }
        }

        return parts;
    }
}

