namespace TeachingRecordSystem.Core.Tests;

public class SearchTextHelperTests
{
    [Theory]
    [InlineData("1 January 2000", true)]
    [InlineData("15 December 1990", true)]
    [InlineData("31 March 2024", true)]
    [InlineData("1/1/2000", true)]
    [InlineData("15/12/1990", true)]
    [InlineData("31/3/2024", true)]
    [InlineData("1 Jan 2000", true)]
    [InlineData("15 Dec 1990", true)]
    [InlineData("31 Mar 2024", true)]
    [InlineData("not a date", false)]
    [InlineData("13/13/2000", false)]
    [InlineData("32/1/2000", false)]
    [InlineData("", false)]
    [InlineData("2000-01-01", false)]
    public void IsDate_WithVariousInputs_ReturnsExpectedResult(string searchText, bool expectedResult)
    {
        // Arrange

        // Act
        var result = SearchTextHelper.IsDate(searchText, out var date);

        // Assert
        Assert.Equal(expectedResult, result);
        if (expectedResult)
        {
            Assert.NotEqual(default(DateOnly), date);
        }
    }

    [Fact]
    public void IsDate_WithValidDate_ParsesDateCorrectly()
    {
        // Arrange
        var searchText = "1 January 2000";

        // Act
        var result = SearchTextHelper.IsDate(searchText, out var date);

        // Assert
        Assert.True(result);
        Assert.Equal(new DateOnly(2000, 1, 1), date);
    }

    [Fact]
    public void IsDate_WithValidDateShortFormat_ParsesDateCorrectly()
    {
        // Arrange
        var searchText = "15/12/1990";

        // Act
        var result = SearchTextHelper.IsDate(searchText, out var date);

        // Assert
        Assert.True(result);
        Assert.Equal(new DateOnly(1990, 12, 15), date);
    }

    [Fact]
    public void IsDate_WithValidDateAbbreviatedMonth_ParsesDateCorrectly()
    {
        // Arrange
        var searchText = "31 Mar 2024";

        // Act
        var result = SearchTextHelper.IsDate(searchText, out var date);

        // Assert
        Assert.True(result);
        Assert.Equal(new DateOnly(2024, 3, 31), date);
    }

    [Theory]
    [InlineData("user@example.com", true)]
    [InlineData("test.user@domain.co.uk", true)]
    [InlineData("name+tag@email.com", true)]
    [InlineData("simple@test.org", true)]
    [InlineData("no-at-sign", false)]
    [InlineData("multiple@@signs.com", true)]
    [InlineData("", false)]
    [InlineData("@", true)]
    [InlineData("user@", true)]
    [InlineData("@domain.com", true)]
    public void IsEmailAddress_WithVariousInputs_ReturnsExpectedResult(string searchText, bool expectedResult)
    {
        // Arrange

        // Act
        var result = SearchTextHelper.IsEmailAddress(searchText, out var email);

        // Assert
        Assert.Equal(expectedResult, result);
        if (expectedResult)
        {
            Assert.Equal(searchText, email);
        }
        else
        {
            Assert.Null(email);
        }
    }

    [Theory]
    [InlineData("TRS-123", true)]
    [InlineData("TRS-456789", true)]
    [InlineData("trs-123", true)]
    [InlineData("Trs-456", true)]
    [InlineData("TRS-", true)]
    [InlineData("RS-123", false)]
    [InlineData("TR-123", false)]
    [InlineData("123", false)]
    [InlineData("", false)]
    [InlineData("TRN-123", false)]
    public void IsSupportTaskReference_WithVariousInputs_ReturnsExpectedResult(string searchText, bool expectedResult)
    {
        // Arrange

        // Act
        var result = SearchTextHelper.IsSupportTaskReference(searchText);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("1234567", true)]
    [InlineData("9876543", true)]
    [InlineData("0000000", true)]
    [InlineData("123456", false)]
    [InlineData("12345678", false)]
    [InlineData("123456a", false)]
    [InlineData("abcdefg", false)]
    [InlineData("", false)]
    [InlineData("1 234567", false)]
    [InlineData("1234567 ", false)]
    [InlineData(" 1234567", false)]
    public void IsTrn_WithVariousInputs_ReturnsExpectedResult(string searchText, bool expectedResult)
    {
        // Arrange

        // Act
        var result = SearchTextHelper.IsTrn(searchText);

        // Assert
        Assert.Equal(expectedResult, result);
    }
}
