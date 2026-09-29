namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class ChangeLogTrnAllocationProcessTests : TestBase
{
    public ChangeLogTrnAllocationProcessTests(HostFixture hostFixture) : base(hostFixture)
    {
        // Toggle between GMT and BST to ensure we're testing rendering dates in local time
        var nows = new[]
        {
            new DateTime(2024, 1, 1, 12, 13, 14, DateTimeKind.Utc),  // GMT
            new DateTime(2024, 7, 5, 19, 20, 21, DateTimeKind.Utc)   // BST
        };
        TimeProvider.SetUtcNow(new DateTimeOffset(nows.SingleRandom(), TimeSpan.Zero));
    }

    [Fact]
    public async Task Person_WithTrnAllocatingProcess_RendersExpectedContent()
    {
        // Arrange
        var user = await TestData.CreateUserAsync();
        var person = await TestData.CreatePersonAsync();

        var process = await TestData.CreateProcessAsync(
            ProcessType.TrnAllocating,
            user.UserId,
            changeReason: null,
            new TrnAllocatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Trn = person.Trn!
            });

        // Act
        var response = await HttpClient.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "TRN allocated",
            user.Name,
            process.CreatedOn,
            ("TRN", person.Trn));
    }
}
