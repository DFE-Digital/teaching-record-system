using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class GetQualificationsByContactIdTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public GetQualificationsByContactIdTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task WhenCalled_ForContactWithoutQualifications_ReturnsEmptyArray()
    {
        // Arrange
        var person = await _dataScope.TestData.CreatePersonAsync();

        // Act
        var qualifications = await _crmQueryDispatcher.ExecuteQueryAsync(new GetQualificationsByContactIdQuery(person.ContactId, new ColumnSet()));

        // Assert
        Assert.Empty(qualifications);
    }

    [Fact]
    public async Task WhenCalled_ForContactWithQualifications_ReturnsQualificationsAsExpected()
    {
        // Arrange
        var activeNpqQualificationId = Guid.NewGuid();
        var activeNpqQualificationType = dfeta_qualification_dfeta_Type.NPQLT;
        var inActiveNpqQualificationId = Guid.NewGuid();
        var inActiveNpqQualificationType = dfeta_qualification_dfeta_Type.NPQH;
        var activeHeQualificationId = Guid.NewGuid();
        var inActiveHeQualificationId = Guid.NewGuid();
        var activeHeQualificationWithNoSubjectsId = Guid.NewGuid();

        var person = await _dataScope.TestData.CreatePersonAsync(
            x => x.WithQualification(activeNpqQualificationId, activeNpqQualificationType, isActive: true)
                .WithQualification(inActiveNpqQualificationId, inActiveNpqQualificationType, isActive: false)
                .WithQualification(activeHeQualificationId, dfeta_qualification_dfeta_Type.HigherEducation, isActive: true, heSubject1Value: "100035")
                .WithQualification(inActiveHeQualificationId, dfeta_qualification_dfeta_Type.HigherEducation, isActive: false, heSubject1Value: "100037")
                .WithQualification(activeHeQualificationWithNoSubjectsId, dfeta_qualification_dfeta_Type.HigherEducation, isActive: true));

        // Act
        var qualifications = await _crmQueryDispatcher.ExecuteQueryAsync(new GetQualificationsByContactIdQuery(person.ContactId, new ColumnSet()));

        // Assert
        Assert.Equal(2, qualifications.Length);
        Assert.Contains(activeNpqQualificationId, qualifications.Select(q => q.Id));
        Assert.Contains(activeHeQualificationId, qualifications.Select(q => q.Id));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task WhenCalledWithIncludeHigherEducationDetails_ForContactWithHeQualifications_ReturnsHeQualificationDetailsAsExpected(bool includeHeEducationDetails)
    {
        // Arrange
        var activeHeQualificationId = Guid.NewGuid();
        var activeHeQualificationWithNoSubjectsId = Guid.NewGuid();

        var person = await _dataScope.TestData.CreatePersonAsync(
            x => x.WithQualification(
                activeHeQualificationId,
                dfeta_qualification_dfeta_Type.HigherEducation,
                isActive: true,
                heQualificationValue: "215",
                heSubject1Value: "100035",
                heSubject2Value: "100036",
                heSubject3Value: "100037"));

        // Act
        var qualifications = await _crmQueryDispatcher.ExecuteQueryAsync(
            new GetQualificationsByContactIdQuery(person.ContactId, new ColumnSet(), IncludeHigherEducationDetails: includeHeEducationDetails));

        // Assert
        Assert.Single(qualifications);
        Assert.Contains(activeHeQualificationId, qualifications.Select(q => q.Id));
        var heQualification = qualifications[0].Extract<dfeta_hequalification>();
        var heSubject1 = qualifications[0].Extract<dfeta_hesubject>($"{nameof(dfeta_hesubject)}1", dfeta_hesubject.PrimaryIdAttribute);
        var heSubject2 = qualifications[0].Extract<dfeta_hesubject>($"{nameof(dfeta_hesubject)}2", dfeta_hesubject.PrimaryIdAttribute);
        var heSubject3 = qualifications[0].Extract<dfeta_hesubject>($"{nameof(dfeta_hesubject)}3", dfeta_hesubject.PrimaryIdAttribute);

        if (includeHeEducationDetails)
        {
            Assert.NotNull(heQualification);
            Assert.NotNull(heSubject1);
            Assert.NotNull(heSubject2);
            Assert.NotNull(heSubject3);
        }
        else
        {
            Assert.Null(heQualification);
            Assert.Null(heSubject1);
            Assert.Null(heSubject2);
            Assert.Null(heSubject3);
        }
    }
}
