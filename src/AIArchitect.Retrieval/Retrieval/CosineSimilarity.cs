namespace AIArchitect.Retrieval.Retrieval;

/// <summary>
/// Measures semantic-vector alignment independently of vector magnitude. This is appropriate for
/// text embeddings because retrieval is interested in direction (meaning) rather than raw length.
/// </summary>
public static class CosineSimilarity
{
    public static double Calculate(ReadOnlySpan<float> left, ReadOnlySpan<float> right)
    {
        if (left.IsEmpty)
        {
            throw new ArgumentException("The left vector cannot be empty.", nameof(left));
        }

        if (right.IsEmpty)
        {
            throw new ArgumentException("The right vector cannot be empty.", nameof(right));
        }

        if (left.Length != right.Length)
        {
            throw new ArgumentException(
                $"Vector dimensions must match; received {left.Length} and {right.Length}.",
                nameof(right));
        }

        // Double accumulators reduce rounding error when summing long float embedding vectors.
        double dotProduct = 0;
        double leftMagnitudeSquared = 0;
        double rightMagnitudeSquared = 0;

        for (int index = 0; index < left.Length; index++)
        {
            float leftValue = left[index];
            float rightValue = right[index];

            if (!float.IsFinite(leftValue) || !float.IsFinite(rightValue))
            {
                throw new ArgumentException("Vectors must contain only finite values.");
            }

            dotProduct += (double)leftValue * rightValue;
            leftMagnitudeSquared += (double)leftValue * leftValue;
            rightMagnitudeSquared += (double)rightValue * rightValue;
        }

        if (leftMagnitudeSquared == 0 || rightMagnitudeSquared == 0)
        {
            throw new ArgumentException(
                "Cosine similarity is undefined for a zero-magnitude vector.");
        }

        double similarity = dotProduct / Math.Sqrt(leftMagnitudeSquared * rightMagnitudeSquared);

        // Floating-point rounding can produce values infinitesimally outside the mathematical
        // range. Clamping keeps downstream ranking and display logic within [-1, 1].
        return Math.Clamp(similarity, -1d, 1d);
    }
}

