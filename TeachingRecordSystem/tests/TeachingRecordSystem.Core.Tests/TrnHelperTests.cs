namespace TeachingRecordSystem.Core.Tests;

public class TrnHelperTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("1234567", "1234567")]
    [InlineData("01/23456", "0123456")]
    [InlineData("123 4567", "1234567")]
    [InlineData("12-34567", "1234567")]
    public void NormalizeTrn_WithVariousInputs_ReturnsExpectedResult(string? input, string? expected)
    {
        // Arrange

        // Act
        var result = TrnHelper.NormalizeTrn(input);

        // Assert
        Assert.Equal(expected, result);
    }
}
