using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Tests.DataStore.Postgres.Models;

public class MandatoryQualificationProviderTests
{
    [Theory]
    [InlineData("963", true)]
    [InlineData("957", true)]
    [InlineData("150", true)]
    [InlineData("961", true)]
    [InlineData("210", true)]
    [InlineData("180", true)]
    [InlineData("160", true)]
    [InlineData("120", true)]
    [InlineData("20", true)]
    [InlineData("30", true)]
    [InlineData("955", true)]
    [InlineData("956", true)]
    [InlineData("964", true)]
    [InlineData("959", true)]
    [InlineData("962", true)]
    [InlineData("90", true)]
    [InlineData("965", true)]
    [InlineData("140", true)]
    [InlineData("958", true)]
    [InlineData("960", true)]
    [InlineData("50", true)]
    [InlineData("951", true)]
    [InlineData("954", true)]
    [InlineData("952", false)]
    [InlineData("100", false)]
    [InlineData("110", false)]
    [InlineData("60", false)]
    [InlineData("10", false)]
    [InlineData("130", false)]
    [InlineData("70", false)]
    [InlineData("170", false)]
    [InlineData("80", false)]
    [InlineData("200", false)]
    [InlineData("190", false)]
    [InlineData("40", false)]
    [InlineData("220", false)]
    [InlineData("953", false)]
    [InlineData("240", false)]
    [InlineData("230", false)]
    [InlineData("950", false)]
    public void TryMapFromDqtMqEstablishment_ReturnsExpectedResult(string mqestablishmentValue, bool expectedResult)
    {
        // Arrange

        // Act
        var result = MandatoryQualificationProvider.TryMapFromDqtMqEstablishmentValue(mqestablishmentValue, out var provider);

        // Assert
        Assert.Equal(expectedResult, result);
    }
}
