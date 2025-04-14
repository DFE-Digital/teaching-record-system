
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.Api.UnitTests.V3;

[Collection(nameof(DisableParallelization))]
public class GetTrnRequestTests(OperationTestFixture operationTestFixture) : OperationTestBase(operationTestFixture), IAsyncLifetime
{
    //
    [Fact]
    public Task HandleAsync_RequestDoesNotExist_ReturnsError() =>
        WithHandler<GetTrnRequestHandler>(async handler =>
        {
            // Arrange
            var requestId = Guid.NewGuid().ToString();

            var command = new GetTrnRequestCommand(requestId);

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            AssertError(result, ApiError.ErrorCodes.TrnRequestDoesNotExist);
        });

    [Fact]
    public Task HandleAsync_CreatedContactHasTrn_ReturnsTrnAndCompletedStatus() =>
        WithHandler<GetTrnRequestHandler>(async handler =>
        {
            // Arrange
            var requestId = Guid.NewGuid().ToString();
            var (applicationUserId, _) = CurrentUserProvider.GetCurrentApplicationUser();

            var person = await TestData.CreatePersonAsync(p => p
                .WithTrn()
                .WithTrnRequest(applicationUserId, requestId));

            var command = new GetTrnRequestCommand(requestId);

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            var success = result.GetSuccess();
            Assert.Equal(TrnRequestStatus.Completed, success.Status);
            Assert.Equal(person.Trn, success.Trn);
        });

    [Fact]
    public Task HandleAsync_CreatedContactIsMerged_ReturnsMergedRecordsTrnAndCompletedStatus() =>
        WithHandler<GetTrnRequestHandler>(async handler =>
        {
            // Arrange
            var requestId = Guid.NewGuid().ToString();
            var (applicationUserId, _) = CurrentUserProvider.GetCurrentApplicationUser();
            var firstName = Faker.Name.First();
            var middleName = Faker.Name.Middle();
            var lastName = Faker.Name.Last();
            var dateOfBirth = new DateOnly(1990, 01, 01);
            var email = Faker.Internet.Email();
            var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

            var masterContact = await TestData.CreatePersonAsync(p => p
                .WithTrn()
                .WithFirstName(firstName)
                .WithMiddleName(middleName)
                .WithLastName(lastName)
                .WithDateOfBirth(dateOfBirth)
                .WithEmail(email)
                .WithNationalInsuranceNumber(nationalInsuranceNumber: nationalInsuranceNumber));

            var existingContact = await TestData.CreatePersonAsync(p => p
                .WithoutTrn()
                .WithFirstName(firstName)
                .WithMiddleName(middleName)
                .WithLastName(lastName)
                .WithDateOfBirth(dateOfBirth)
                .WithEmail(email)
                .WithNationalInsuranceNumber(nationalInsuranceNumber: nationalInsuranceNumber)
                .WithTrnRequest(applicationUserId, requestId));

            XrmFakedContext.UpdateEntity(new Contact()
            {
                ContactId = existingContact.ContactId,
                Merged = true,
                MasterId = masterContact.ContactId.ToEntityReference(Contact.EntityLogicalName),
                StateCode = ContactState.Inactive
            });

            var command = new GetTrnRequestCommand(requestId);

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            var success = result.GetSuccess();
            Assert.Equal(TrnRequestStatus.Completed, success.Status);
            Assert.Equal(masterContact.Trn, success.Trn);
        });

    [Fact]
    public Task HandleAsync_CreatedContactDoesNotHaveTrn_ReturnsPendingStatus() =>
        WithHandler<GetTrnRequestHandler>(async handler =>
        {
            // Arrange
            var requestId = Guid.NewGuid().ToString();
            var (applicationUserId, _) = CurrentUserProvider.GetCurrentApplicationUser();

            var person = await TestData.CreatePersonAsync(p => p
                .WithoutTrn()
                .WithTrnRequest(applicationUserId, requestId));

            var command = new GetTrnRequestCommand(requestId);

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            var success = result.GetSuccess();
            Assert.Equal(TrnRequestStatus.Pending, success.Status);
            Assert.Null(success.Trn);
        });

    public async Task InitializeAsync()
    {
        // Any existing Contacts will affect our duplicate matching; clear them all out before every test
        await OperationTestFixture.DbFixture.DbHelper.ClearDataAsync();
        XrmFakedContext.DeleteAllEntities<Contact>();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

