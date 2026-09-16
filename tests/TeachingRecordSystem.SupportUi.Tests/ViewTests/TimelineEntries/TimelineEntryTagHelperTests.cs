using System.Globalization;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Razor.TagHelpers;
using TeachingRecordSystem.SupportUi.TagHelpers;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.TimelineEntries;

public class TimelineEntryTagHelperTests
{
    [Theory]
    [InlineData("2024-01-15T14:30:00Z", "2024-01-15T14:30:00")]
    [InlineData("2024-07-15T14:30:00Z", "2024-07-15T15:30:00")]
    public async Task ProcessAsync_RendersTimestampInGmt(string utcTimestamp, string expectedGmtTimestamp)
    {
        // Arrange
        var tagHelper = new TimelineEntryTagHelper
        {
            Title = "Something happened",
            By = "Joe Bloggs",
            Timestamp = ParseUtc(utcTimestamp)
        };

        var context = new TagHelperContext([], new Dictionary<object, object>(), Guid.NewGuid().ToString());

        var output = new TagHelperOutput(
            "timeline-entry",
            [],
            (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var expected = DateTime.Parse(expectedGmtTimestamp, CultureInfo.InvariantCulture).ToString("d MMMMM yyyy 'at' h:mm tt");
        Assert.Contains(expected, GetHtml(output));
    }

    [Fact]
    public async Task ProcessAsync_RendersTimestampAttributeInUtc()
    {
        // Arrange
        var tagHelper = new TimelineEntryTagHelper
        {
            Title = "Something happened",
            By = "Joe Bloggs",
            Timestamp = ParseUtc("2024-07-15T14:30:00Z")
        };

        var context = new TagHelperContext([], new Dictionary<object, object>(), Guid.NewGuid().ToString());

        var output = new TagHelperOutput(
            "timeline-entry",
            [],
            (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Contains("2024-07-15T14:30:00.0000000Z", GetHtml(output));
    }

    private static DateTime ParseUtc(string timestamp) =>
        DateTime.Parse(timestamp, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);

    private static string GetHtml(TagHelperOutput output)
    {
        using var writer = new StringWriter();
        output.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }
}
