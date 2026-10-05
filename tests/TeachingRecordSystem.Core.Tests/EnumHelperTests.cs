namespace TeachingRecordSystem.Core.Tests;

public class EnumHelperTests
{
    [Theory]
    [InlineData(TestFlags.A, TestFlags.A, true)]
    [InlineData(TestFlags.A | TestFlags.B, TestFlags.B, true)]
    [InlineData(TestFlags.A, TestFlags.B, false)]
    [InlineData(TestFlags.None, TestFlags.A, false)]
    [InlineData(TestFlags.A, TestFlags.None, false)]
    // A composite defined by negation sets the sign bit, so the overlap can be negative
    [InlineData(TestFlags.B, TestFlags.AllButA, true)]
    [InlineData(TestFlags.A, TestFlags.AllButA, false)]
    [InlineData(TestFlags.AllButA, TestFlags.AllButA, true)]
    [InlineData(TestFlags.A | TestFlags.AllButA, TestFlags.AllButA, true)]
    [InlineData(TestFlags.SignBit, TestFlags.SignBit, true)]
    public void HasAnyFlag_ReturnsExpectedResult(TestFlags value, TestFlags flags, bool expected)
    {
        // Arrange

        // Act
        var result = value.HasAnyFlag(flags);

        // Assert
        Assert.Equal(expected, result);
    }

    [Flags]
    public enum TestFlags
    {
        None = 0,
        A = 1 << 0,
        B = 1 << 1,
        SignBit = 1 << 31,
        AllButA = ~None & ~A
    }
}
