using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.Core.Tests.Services.TrsDataSync;

public partial class TrsDataSyncHelperTests
{
    [Theory]
    [InlineData(true, dfeta_InductionStatus.Exempt, InductionStatus.Exempt)]
    [InlineData(true, dfeta_InductionStatus.PassedinWales, InductionStatus.Exempt)]
    [InlineData(true, dfeta_InductionStatus.Fail, InductionStatus.Failed)]
    [InlineData(true, dfeta_InductionStatus.FailedinWales, InductionStatus.FailedInWales)]
    [InlineData(true, dfeta_InductionStatus.InProgress, InductionStatus.InProgress)]
    [InlineData(true, dfeta_InductionStatus.InductionExtended, InductionStatus.InProgress)]
    [InlineData(true, dfeta_InductionStatus.NotYetCompleted, InductionStatus.InProgress)]
    [InlineData(true, dfeta_InductionStatus.Pass, InductionStatus.Passed)]
    [InlineData(true, dfeta_InductionStatus.RequiredtoComplete, InductionStatus.RequiredToComplete)]
    [InlineData(false, dfeta_InductionStatus.Exempt, InductionStatus.Exempt)]
    [InlineData(false, dfeta_InductionStatus.PassedinWales, InductionStatus.Exempt)]
    [InlineData(false, dfeta_InductionStatus.Fail, InductionStatus.Failed)]
    [InlineData(false, dfeta_InductionStatus.FailedinWales, InductionStatus.FailedInWales)]
    [InlineData(false, dfeta_InductionStatus.InProgress, InductionStatus.InProgress)]
    [InlineData(false, dfeta_InductionStatus.InductionExtended, InductionStatus.InProgress)]
    [InlineData(false, dfeta_InductionStatus.NotYetCompleted, InductionStatus.InProgress)]
    [InlineData(false, dfeta_InductionStatus.Pass, InductionStatus.Passed)]
    [InlineData(false, dfeta_InductionStatus.RequiredtoComplete, InductionStatus.RequiredToComplete)]
    public async Task SyncInductionsAsync_WithInduction_UpdatesPersonRecord(bool personAlreadySynced, dfeta_InductionStatus dqtInductionStatus, InductionStatus expectedTrsInductionStatus)
    {
        // Arrange
        var inductionExemptionReason = dqtInductionStatus == dfeta_InductionStatus.Exempt ? dfeta_InductionExemptionReason.Exempt : (dfeta_InductionExemptionReason?)null;
        var inductionStartDate = Clock.Today.AddYears(-1);
        var inductionEndDate = Clock.Today.AddDays(-10);

        var person = await TestData.CreatePersonAsync(
            p => p.WithTrn()
                .WithSyncOverride(personAlreadySynced)
                .WithDqtInduction(dqtInductionStatus, inductionExemptionReason, inductionStartDate, inductionEndDate));

        // Act
        await Helper.SyncInductionsAsync(new[] { person.Contact }, true, dryRun: false, CancellationToken.None);

        // Assert
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var updatedPerson = await dbContext.Persons.SingleAsync(p => p.DqtContactId == person.ContactId);
            Assert.Equal(expectedTrsInductionStatus, updatedPerson!.InductionStatus);
            Assert.Equal(inductionStartDate, updatedPerson.InductionStartDate);
            Assert.Equal(inductionEndDate, updatedPerson.InductionCompletedDate);
        });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SyncInductionsAsync_WithContactOnlyInductionStatus_UpdatesPersonRecord(bool personAlreadySynced)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(
            p => p.WithTrn()
                .WithSyncOverride(personAlreadySynced));

        await TestData.OrganizationService.UpdateAsync(new Contact()
        {
            Id = person.ContactId,
            dfeta_InductionStatus = dfeta_InductionStatus.RequiredtoComplete
        });

        using var ctx = new DqtCrmServiceContext(TestData.OrganizationService);
        var contact = ctx.ContactSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(Contact.PrimaryIdAttribute) == person.ContactId);

        // Act
        await Helper.SyncInductionsAsync(new[] { contact! }, true, dryRun: false, CancellationToken.None);

        // Assert
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var updatedPerson = await dbContext.Persons.SingleOrDefaultAsync(p => p.DqtContactId == person.ContactId);
            Assert.Equal(InductionStatus.RequiredToComplete, updatedPerson!.InductionStatus);
        });
    }

    [Fact]
    public async Task SyncInductionsAsync_WithQtlsButNotExemptAndIgnoreInvalidSetToFalse_ThrowsException()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(
            p => p.WithTrn()
                .WithQts()
                .WithQtlsDate(Clock.Today)
                .WithDqtInduction(dfeta_InductionStatus.RequiredtoComplete, null, null, null)
                .WithSyncOverride(true));

        using var ctx = new DqtCrmServiceContext(TestData.OrganizationService);
        var contact = ctx.ContactSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(Contact.PrimaryIdAttribute) == person.ContactId);

        // Act
        var exception = await Record.ExceptionAsync(() => Helper.SyncInductionsAsync(new[] { contact! }, false, dryRun: false, CancellationToken.None));

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public async Task SyncInductionsAsync_WithQtls_UpdatesPersonRecord()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(
            p => p.WithTrn()
                .WithQtlsDate(Clock.Today)
                .WithSyncOverride(true));

        using var ctx = new DqtCrmServiceContext(TestData.OrganizationService);
        var contact = ctx.ContactSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(Contact.PrimaryIdAttribute) == person.ContactId);

        // Act
        await Helper.SyncInductionsAsync(new[] { contact! }, true, dryRun: false, CancellationToken.None);

        // Assert
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var updatedPerson = await dbContext.Persons.SingleOrDefaultAsync(p => p.DqtContactId == person.ContactId);
            Assert.Equal(InductionStatus.Exempt, updatedPerson!.InductionStatus);
        });
    }
}
