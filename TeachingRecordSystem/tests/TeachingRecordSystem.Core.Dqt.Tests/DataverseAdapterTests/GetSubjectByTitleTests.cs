#nullable disable

namespace TeachingRecordSystem.Core.Dqt.Tests.DataverseAdapterTests;

public class GetSubjectByTitleTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly DataverseAdapter _dataverseAdapter;

    public GetSubjectByTitleTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _dataverseAdapter = _dataScope.CreateDataverseAdapter();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task ValidDescription_ReturnsSubject()
    {
        // Arrange
        var title = "Change of Name";

        // Act
        var result = await _dataverseAdapter.GetSubjectByTitle(title);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(title, result.Title);
    }

    [Fact]
    public async Task UnmatchedDescription_ReturnsNull()
    {
        // Arrange
        var subjectCode = "XXXX";

        // Act
        var result = await _dataverseAdapter.GetSubjectByTitle(subjectCode);

        // Assert
        Assert.Null(result);
    }
}
