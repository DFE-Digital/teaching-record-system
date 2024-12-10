using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Tests.Services.TrsDataSync;

public partial class TrsDataSyncServiceTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Induction_NewRecord_WritesUpdatedPersonRecordToDatabase(bool personAlreadySynced)
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync(p => p.WithSyncOverride(personAlreadySynced));
        var contactId = createPersonResult.ContactId;

        var inductionStartDate = Clock.Today.AddYears(-1);
        var inductionEndDate = Clock.Today.AddDays(-10);
        var induction = new dfeta_induction()
        {
            Id = Guid.NewGuid(),
            dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, contactId),
            dfeta_InductionStatus = dfeta_InductionStatus.Pass,
            dfeta_StartDate = inductionStartDate.ToDateTimeWithDqtBstFix(isLocalTime: true),
            dfeta_CompletionDate = inductionEndDate.ToDateTimeWithDqtBstFix(isLocalTime: true),
            CreatedOn = Clock.UtcNow,
            ModifiedOn = Clock.UtcNow
        };

        // Keep the contact induction status in sync with dfeta_induction otherwise the sync will fail
        await TestData.OrganizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new Contact()
            {
                Id = contactId,
                dfeta_InductionStatus = dfeta_InductionStatus.Pass
            }
        });

        var newItem = new NewOrUpdatedItem(ChangeType.NewOrUpdated, induction);

        // Act
        await fixture.PublishChangedItemAndConsumeAsync(TrsDataSyncHelper.ModelTypes.Induction, newItem);

        // Assert
        await fixture.DbFixture.WithDbContextAsync(async dbContext =>
        {
            var person = await dbContext.Persons.SingleOrDefaultAsync(p => p.DqtContactId == contactId);
            Assert.NotNull(person);
            Assert.Equal(InductionStatus.Passed, person.InductionStatus);
            Assert.Equal(inductionStartDate, person.InductionStartDate);
            Assert.Equal(inductionEndDate, person.InductionCompletedDate);
        });
    }

    [Fact]
    public async Task Induction_UpdatedRecord_WritesUpdatedPersonRecordToDatabase()
    {
        // Arrange
        var originalInductionStatus = dfeta_InductionStatus.InProgress;
        var originalInductionStartDate = Clock.Today.AddYears(-1);
        var originalInductionEndDate = Clock.Today.AddDays(-10);

        var createPersonResult = await TestData.CreatePersonAsync(
            p => p.WithSyncOverride(true)
                .WithDqtInduction(originalInductionStatus, null, originalInductionStartDate, null));
        var contactId = createPersonResult.ContactId;
        var existingInduction = createPersonResult.DqtInductions.Single();

        var updatedInductionStatus = dfeta_InductionStatus.Pass;
        var updatedInductionStartDate = Clock.Today.AddYears(-2);
        var updatedInductionEndDate = Clock.Today.AddDays(-20);
        var createdOn = Clock.UtcNow;
        var modifiedOn = Clock.Advance();
        var updatedInduction = new dfeta_induction()
        {
            Id = existingInduction.InductionId,
            dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, contactId),
            dfeta_InductionStatus = updatedInductionStatus,
            dfeta_StartDate = updatedInductionStartDate.ToDateTimeWithDqtBstFix(isLocalTime: true),
            dfeta_CompletionDate = updatedInductionEndDate.ToDateTimeWithDqtBstFix(isLocalTime: true),
            CreatedOn = Clock.UtcNow,
            ModifiedOn = modifiedOn
        };

        // Keep the contact induction status in sync with dfeta_induction otherwise the sync will fail
        await TestData.OrganizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new Contact()
            {
                Id = contactId,
                dfeta_InductionStatus = dfeta_InductionStatus.Pass
            }
        });

        var updatedItem = new NewOrUpdatedItem(ChangeType.NewOrUpdated, updatedInduction);

        // Act
        await fixture.PublishChangedItemAndConsumeAsync(TrsDataSyncHelper.ModelTypes.Induction, updatedItem);

        // Assert
        await fixture.DbFixture.WithDbContextAsync(async dbContext =>
        {
            var person = await dbContext.Persons.SingleOrDefaultAsync(p => p.DqtContactId == contactId);
            Assert.NotNull(person);
            Assert.Equal(InductionStatus.Passed, person.InductionStatus);
            Assert.Equal(updatedInductionStartDate, person.InductionStartDate);
            Assert.Equal(updatedInductionEndDate, person.InductionCompletedDate);
            Assert.Equal(Clock.UtcNow, person.DqtInductionModifiedOn);
        });
    }
}
