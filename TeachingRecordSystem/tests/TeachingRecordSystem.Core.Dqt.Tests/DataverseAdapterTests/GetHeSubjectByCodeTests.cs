#nullable disable

namespace TeachingRecordSystem.Core.Dqt.Tests.DataverseAdapterTests;

public class GetHeSubjectByCodeTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly DataverseAdapter _dataverseAdapter;

    public GetHeSubjectByCodeTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _dataverseAdapter = _dataScope.CreateDataverseAdapter();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task Given_valid_subject_name_returns_country()
    {
        // Arrange
        var subjectCode = "100366";  // computer science

        // Act
        var result = await _dataverseAdapter.GetHeSubjectByCode(subjectCode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(subjectCode, result.dfeta_Value);
    }

    [Fact]
    public async Task Given_invalid_subject_name_returns_null()
    {
        // Arrange
        var subjectCode = "XXXX";

        // Act
        var result = await _dataverseAdapter.GetHeSubjectByCode(subjectCode);

        // Assert
        Assert.Null(result);
    }
}
