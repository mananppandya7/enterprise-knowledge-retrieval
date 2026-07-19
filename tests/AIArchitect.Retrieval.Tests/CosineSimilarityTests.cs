using AIArchitect.Retrieval.Retrieval;

namespace AIArchitect.Retrieval.Tests;

public sealed class CosineSimilarityTests
{
    [Fact]
    public void Calculate_IdenticalVectors_ReturnsOne()
    {
        double result = CosineSimilarity.Calculate([1f, 2f, 3f], [1f, 2f, 3f]);

        Assert.Equal(1d, result, precision: 12);
    }

    [Fact]
    public void Calculate_OrthogonalVectors_ReturnsZero()
    {
        double result = CosineSimilarity.Calculate([1f, 0f], [0f, 1f]);

        Assert.Equal(0d, result, precision: 12);
    }

    [Fact]
    public void Calculate_OppositeVectors_ReturnsNegativeOne()
    {
        double result = CosineSimilarity.Calculate([1f, -2f], [-1f, 2f]);

        Assert.Equal(-1d, result, precision: 12);
    }

    [Fact]
    public void Calculate_ScaledVectors_ReturnsSameDirectionScore()
    {
        double result = CosineSimilarity.Calculate([1f, 2f, 3f], [10f, 20f, 30f]);

        Assert.Equal(1d, result, precision: 12);
    }

    [Fact]
    public void Calculate_MismatchedDimensions_Throws()
    {
        float[] left = [1f, 2f];
        float[] right = [1f];

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => CosineSimilarity.Calculate(left, right));

        Assert.Contains("dimensions must match", exception.Message);
    }

    [Fact]
    public void Calculate_ZeroVector_Throws()
    {
        float[] zero = [0f, 0f];
        float[] nonZero = [1f, 2f];

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => CosineSimilarity.Calculate(zero, nonZero));

        Assert.Contains("zero-magnitude", exception.Message);
    }

    [Theory]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    public void Calculate_NonFiniteValue_Throws(float invalidValue)
    {
        float[] left = [1f, invalidValue];
        float[] right = [1f, 2f];

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => CosineSimilarity.Calculate(left, right));

        Assert.Contains("finite values", exception.Message);
    }
}
