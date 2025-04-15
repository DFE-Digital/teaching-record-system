using System.Diagnostics;
using FakeXrmEasy.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Services.DqtOutbox.Messages;
using TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;

#pragma warning disable TRS0001

namespace TeachingRecordSystem.Api.UnitTests.V3;

[Collection(nameof(DisableParallelization))]
public class CreateTrnRequestTests(OperationTestFixture operationTestFixture) : OperationTestBase(operationTestFixture), IAsyncLifetime
{
    [Fact]
    public Task HandleAsync_RequestForSameUserAndIdAlreadyExists_ReturnsError() =>
        WithHandler<CreateTrnRequestHandler>(async handler =>
        {
            // Arrange
            var command = CreateCommand();
            var applicationUserId = CurrentUserProvider.GetCurrentApplicationUser().UserId;

            await TestData.CreatePersonAsync(p => p
                .WithTrn()
                .WithFirstName(command.FirstName)
                .WithMiddleName(command.MiddleName)
                .WithLastName(command.LastName)
                .WithDateOfBirth(command.DateOfBirth)
                .WithNationalInsuranceNumber(command.NationalInsuranceNumber!)
                .WithEmail(command.EmailAddresses.First())
                .WithTrnRequest(applicationUserId, command.RequestId));

            // Act
            var result = await handler.HandleAsync(command);
            AssertError(result, 10029);  // Cannot resubmit request
        });

    [Fact]
    public Task HandleAsync_WithMultipleFirstNames_NormalizesFirstAndMiddleNames() =>
        WithHandler<CreateTrnRequestHandler>(async handler =>
        {
            // Arrange
            var firstName1 = TestData.GenerateFirstName();
            var firstName2 = TestData.GenerateFirstName();
            var firstName = $"{firstName1} {firstName2}";
            var middleName = TestData.GenerateMiddleName();

            var command = CreateCommand() with
            {
                FirstName = firstName,
                MiddleName = middleName
            };

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            var (createContactQuery, _) = CrmQueryDispatcherSpy.GetSingleQuery<CreateContactQuery, Guid>();
            Assert.Equal(firstName1, createContactQuery.FirstName);
            Assert.Equal(firstName, createContactQuery.StatedFirstName);
            Assert.Equal($"{firstName2} {middleName}", createContactQuery.MiddleName);
            Assert.Equal(middleName, createContactQuery.StatedMiddleName);
        });

    [Fact]
    public Task HandleAsync_WithNino_NormalizesNino() =>
        WithHandler<CreateTrnRequestHandler>(async handler =>
        {
            // Arrange
            var nationalInsuranceNumber = "WC 34 87 05 C";
            var expectedNormalizedInsuranceNumber = "WC348705C";

            var command = CreateCommand() with
            {
                NationalInsuranceNumber = nationalInsuranceNumber
            };

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            var (createContactQuery, _) = CrmQueryDispatcherSpy.GetSingleQuery<CreateContactQuery, Guid>();
            Assert.Equal(expectedNormalizedInsuranceNumber, createContactQuery.NationalInsuranceNumber);
        });

    [Fact]
    public Task HandleAsync_WithNoEmail_Succeeds() =>
        WithHandler<CreateTrnRequestHandler>(async handler =>
        {
            // Arrange
            var command = CreateCommand() with
            {
                EmailAddresses = []
            };

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            AssertSuccess(result);
        });

    [Fact]
    public Task HandleAsync_WithNoNino_Succeeds() =>
        WithHandler<CreateTrnRequestHandler>(async handler =>
        {
            // Arrange
            var command = CreateCommand() with
            {
                NationalInsuranceNumber = null
            };

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            AssertSuccess(result);
        });

    [Theory]
    [MemberData(nameof(GetPotentialMatchCombinationsData))]
    public Task HandleAsync_MatchingExistingPersonOnTwoNamesAndDateOfBirth_ReturnsPendingStatusAndCreatesContactAndReviewTask(MatchedField[] matchedFields) =>
        WithHandler<CreateTrnRequestHandler>(async handler =>
        {
            // Arrange
            var firstName = TestData.GenerateFirstName();
            var middleName = TestData.GenerateMiddleName();
            var lastName = TestData.GenerateLastName();
            var dateOfBirth = TestData.GenerateDateOfBirth();
            var emailAddress = TestData.GenerateUniqueEmail();
            var nino = TestData.GenerateNationalInsuranceNumber();

            var matchedPerson = await TestData.CreatePersonAsync(p => p
                .WithTrn()
                .WithFirstName(matchedFields.Contains(MatchedField.FirstName) ? firstName : TestData.GenerateChangedFirstName(firstName))
                .WithMiddleName(matchedFields.Contains(MatchedField.MiddleName) ? middleName : TestData.GenerateChangedMiddleName(middleName))
                .WithLastName(matchedFields.Contains(MatchedField.LastName) ? lastName : TestData.GenerateChangedLastName(lastName))
                .WithDateOfBirth(matchedFields.Contains(MatchedField.DateOfBirth) ? dateOfBirth : TestData.GenerateChangedDateOfBirth(dateOfBirth))
                .WithEmail(matchedFields.Contains(MatchedField.EmailAddress) ? emailAddress : TestData.GenerateUniqueEmail())
                .WithNationalInsuranceNumber(matchedFields.Contains(MatchedField.DqtNationalInsuranceNumber)
                    ? nino
                    : TestData.GenerateChangedNationalInsuranceNumber(nino)));

            if (matchedFields.Contains(MatchedField.WorkforceNationalInsuranceNumber))
            {
                var establishment = await TestData.CreateEstablishmentAsync(localAuthorityCode: "321");
                await TestData.CreateTpsEmploymentAsync(
                    matchedPerson,
                    establishment,
                    startDate: new DateOnly(2024, 1, 1),
                    lastKnownEmployedDate: new DateOnly(2024, 10, 1),
                    EmploymentType.FullTime,
                    lastExtractDate: new DateOnly(2024, 10, 1),
                    nationalInsuranceNumber: nino);
            }

            var command = CreateCommand() with
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                EmailAddresses = [emailAddress],
                NationalInsuranceNumber = nino
            };

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            var success = AssertSuccess(result);

            Assert.Equal(command.RequestId, success.RequestId);
            Assert.Equal(TrnRequestStatus.Pending, success.Status);
            Assert.Null(success.Trn);
            Assert.Null(success.AccessYourTeachingQualificationsLink);
            AssertResultPersonMatchesCommand(command, success.Person);

            var (createContactQuery, _) = CrmQueryDispatcherSpy.GetSingleQuery<CreateContactQuery, Guid>();
            Assert.Null(createContactQuery.Trn);

            Assert.Collection(
                createContactQuery.ReviewTasks,
                task => AssertReviewTaskCreated(matchedPerson, task));

            AssertContactMatchesCommand(command, createContactQuery);
            AssertMetadataMessageMatchesCommand(command, createContactQuery.TrnRequestMetadataMessage, expectedPotentialDuplicate: true);
        });

    [Fact]
    public Task HandleAsync_MatchingExistingPersonOnDqtNinoAndDob_ReturnsTrnOfExistingPersonDoesNotCreateContact() =>
        WithHandler<CreateTrnRequestHandler>(async handler =>
        {
            // Arrange
            var firstName = Faker.Name.First();
            var middleName = Faker.Name.Middle();
            var lastName = Faker.Name.Last();
            var dateOfBirth = new DateOnly(1990, 01, 01);
            var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

            await TestData.CreatePersonAsync(p => p
                .WithTrn()
                .WithFirstName(TestData.GenerateChangedFirstName(firstName))
                .WithMiddleName(TestData.GenerateChangedMiddleName(middleName))
                .WithLastName(TestData.GenerateChangedLastName(lastName))
                .WithDateOfBirth(dateOfBirth)
                .WithNationalInsuranceNumber(nationalInsuranceNumber));

            var command = CreateCommand() with
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                NationalInsuranceNumber = nationalInsuranceNumber
            };

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            var success = AssertSuccess(result);

            Assert.Equal(command.RequestId, success.RequestId);
            Assert.Equal(TrnRequestStatus.Completed, success.Status);
            Assert.NotNull(success.Trn);
            Assert.NotNull(success.AccessYourTeachingQualificationsLink);
            AssertResultPersonMatchesCommand(command, success.Person);

            Assert.Empty(CrmQueryDispatcherSpy.GetAllQueries<CreateContactQuery, Guid>());
            var (createMetadataMessageQuery, _) = CrmQueryDispatcherSpy.GetSingleQuery<CreateDqtOutboxMessageQuery, Guid>();
            var metadataMessage = Assert.IsType<TrnRequestMetadataMessage>(createMetadataMessageQuery.Message);
            AssertMetadataMessageMatchesCommand(command, metadataMessage, expectedPotentialDuplicate: false);
        });

    [Fact]
    public Task HandleAsync_MatchingExistingPersonOnWorkforceNinoAndDob_ReturnsTrnOfExistingPersonDoesNotCreateContact() =>
        WithHandler<CreateTrnRequestHandler>(async handler =>
        {
            // Arrange
            var firstName = Faker.Name.First();
            var middleName = Faker.Name.Middle();
            var lastName = Faker.Name.Last();
            var dateOfBirth = new DateOnly(1990, 01, 01);
            var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

            var person = await TestData.CreatePersonAsync(p => p
                .WithTrn()
                .WithFirstName(TestData.GenerateChangedFirstName(firstName))
                .WithMiddleName(TestData.GenerateChangedMiddleName(middleName))
                .WithLastName(TestData.GenerateChangedLastName(lastName))
                .WithDateOfBirth(dateOfBirth));
            Debug.Assert(person.NationalInsuranceNumber is null);

            var establishment = await TestData.CreateEstablishmentAsync(localAuthorityCode: "321");
            await TestData.CreateTpsEmploymentAsync(
                person,
                establishment,
                startDate: new DateOnly(2024, 1, 1),
                lastKnownEmployedDate: new DateOnly(2024, 10, 1),
                EmploymentType.FullTime,
                lastExtractDate: new DateOnly(2024, 10, 1),
                nationalInsuranceNumber: nationalInsuranceNumber);

            var command = CreateCommand() with
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                NationalInsuranceNumber = nationalInsuranceNumber
            };

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            var success = AssertSuccess(result);

            Assert.Equal(command.RequestId, success.RequestId);
            Assert.Equal(TrnRequestStatus.Completed, success.Status);
            Assert.NotNull(success.Trn);
            Assert.NotNull(success.AccessYourTeachingQualificationsLink);
            AssertResultPersonMatchesCommand(command, success.Person);

            Assert.Empty(CrmQueryDispatcherSpy.GetAllQueries<CreateContactQuery, Guid>());
            var (createMetadataMessageQuery, _) = CrmQueryDispatcherSpy.GetSingleQuery<CreateDqtOutboxMessageQuery, Guid>();
            var metadataMessage = Assert.IsType<TrnRequestMetadataMessage>(createMetadataMessageQuery.Message);
            AssertMetadataMessageMatchesCommand(command, metadataMessage, expectedPotentialDuplicate: false);
        });

    [Fact]
    public Task HandleAsync_PotentialDuplicateHasAlert_CreatesReviewTaskWithAlertInDescription() =>
        WithHandler<CreateTrnRequestHandler>(async handler =>
        {
            // Arrange
            var firstName = TestData.GenerateFirstName();
            var lastName = TestData.GenerateLastName();
            var dateOfBirth = TestData.GenerateDateOfBirth();

            var matchedPerson = await TestData.CreatePersonAsync(p => p
                .WithTrn()
                .WithFirstName(firstName)
                .WithLastName(lastName)
                .WithDateOfBirth(dateOfBirth)
                .WithAlert());

            var command = CreateCommand() with
            {
                FirstName = firstName,
                LastName = lastName,
                DateOfBirth = dateOfBirth
            };

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            var (createContactQuery, _) = CrmQueryDispatcherSpy.GetSingleQuery<CreateContactQuery, Guid>();
            Assert.Collection(
                createContactQuery.ReviewTasks,
                task => Assert.Contains("active sanctions", task.Description));
        });

    [Fact]
    public Task HandleAsync_PotentialDuplicateHasQts_CreatesReviewTaskWithQtsInDescription() =>
        WithHandler<CreateTrnRequestHandler>(async handler =>
        {
            // Arrange
            var firstName = TestData.GenerateFirstName();
            var lastName = TestData.GenerateLastName();
            var dateOfBirth = TestData.GenerateDateOfBirth();

            var matchedPerson = await TestData.CreatePersonAsync(p => p
                .WithTrn()
                .WithFirstName(firstName)
                .WithLastName(lastName)
                .WithDateOfBirth(dateOfBirth)
                .WithQts());

            var command = CreateCommand() with
            {
                FirstName = firstName,
                LastName = lastName,
                DateOfBirth = dateOfBirth
            };

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            var (createContactQuery, _) = CrmQueryDispatcherSpy.GetSingleQuery<CreateContactQuery, Guid>();
            Assert.Collection(
                createContactQuery.ReviewTasks,
                task => Assert.Contains("has QTS", task.Description));
        });

    [Fact]
    public Task HandleAsync_PotentialDuplicateHasEyts_CreatesReviewTaskWithEytsInDescription() =>
        WithHandler<CreateTrnRequestHandler>(async handler =>
        {
            // Arrange
            var firstName = TestData.GenerateFirstName();
            var lastName = TestData.GenerateLastName();
            var dateOfBirth = TestData.GenerateDateOfBirth();

            var matchedPerson = await TestData.CreatePersonAsync(p => p
                .WithTrn()
                .WithFirstName(firstName)
                .WithLastName(lastName)
                .WithDateOfBirth(dateOfBirth)
                .WithEyts(eytsDate: Clock.Today, eytsStatusValue: "221"));

            var command = CreateCommand() with
            {
                FirstName = firstName,
                LastName = lastName,
                DateOfBirth = dateOfBirth
            };

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            var (createContactQuery, _) = CrmQueryDispatcherSpy.GetSingleQuery<CreateContactQuery, Guid>();
            Assert.Collection(
                createContactQuery.ReviewTasks,
                task => Assert.Contains("has EYTS", task.Description));
        });

    [Fact]
    public Task HandleAsync_NoMatches_CreatesContactWithTrnButNoReviewTask() =>
        WithHandler<CreateTrnRequestHandler>(async handler =>
        {
            // Arrange
            var command = CreateCommand();

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            var success = AssertSuccess(result);

            Assert.Equal(command.RequestId, success.RequestId);
            Assert.Equal(TrnRequestStatus.Completed, success.Status);
            Assert.NotNull(success.Trn);
            Assert.NotNull(success.AccessYourTeachingQualificationsLink);
            AssertResultPersonMatchesCommand(command, success.Person);

            var (createContactQuery, _) = CrmQueryDispatcherSpy.GetSingleQuery<CreateContactQuery, Guid>();
            Assert.NotNull(createContactQuery.Trn);
            Assert.Empty(createContactQuery.ReviewTasks);
            AssertContactMatchesCommand(command, createContactQuery);
            AssertMetadataMessageMatchesCommand(command, createContactQuery.TrnRequestMetadataMessage, expectedPotentialDuplicate: false);
        });

    [Fact]
    public Task HandleAsync_ClientIsInAllowContactPiiUpdatesFromUserIdsConfig_SetsAllowPiiUpdatesFromRegisterToTrue()
    {
        var (applicationUserId, _) = CurrentUserProvider.GetCurrentApplicationUser();

        IConfiguration configuration = new ConfigurationBuilder()
            .AddConfiguration(OperationTestFixture.Services.GetRequiredService<IConfiguration>())
            .AddInMemoryCollection([new KeyValuePair<string, string?>("AllowContactPiiUpdatesFromUserIds:0", $"{applicationUserId}")])
            .Build();

        return WithHandler<CreateTrnRequestHandler>(async handler =>
        {
            // Arrange
            var command = CreateCommand();

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            var (createContactQuery, _) = CrmQueryDispatcherSpy.GetSingleQuery<CreateContactQuery, Guid>();
            Assert.True(createContactQuery.AllowPiiUpdates);
        }, configuration);
    }

    [Fact]
    public Task HandleAsync_ClientIsNotInAllowContactPiiUpdatesFromUserIdsConfig_SetsAllowPiiUpdatesFromRegisterToFalse()
    {
        var (applicationUserId, _) = CurrentUserProvider.GetCurrentApplicationUser();

        IConfiguration configuration = OperationTestFixture.Services.GetRequiredService<IConfiguration>();
        Debug.Assert(!(configuration.GetSection("AllowContactPiiUpdatesFromUserIds").Get<string[]>() ?? []).Contains($"{applicationUserId}"));

        return WithHandler<CreateTrnRequestHandler>(async handler =>
        {
            // Arrange
            var command = CreateCommand();

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            var (createContactQuery, _) = CrmQueryDispatcherSpy.GetSingleQuery<CreateContactQuery, Guid>();
            Assert.False(createContactQuery.AllowPiiUpdates);
        }, configuration);
    }

    private CreateTrnRequestCommand CreateCommand() => new CreateTrnRequestCommand
    {
        RequestId = Guid.NewGuid().ToString(),
        FirstName = TestData.GenerateFirstName(),
        MiddleName = TestData.GenerateMiddleName(),
        LastName = TestData.GenerateLastName(),
        DateOfBirth = TestData.GenerateDateOfBirth(),
        EmailAddresses = [TestData.GenerateUniqueEmail()],
        NationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber(),
        IdentityVerified = null,
        OneLoginUserSubject = null,
        Gender = Enum.GetValues<Gender>().RandomOne()
    };

    private void AssertReviewTaskCreated(
        TestData.CreatePersonResult potentialDuplicate,
        CreateContactQueryDuplicateReviewTask task)
    {
        var (_, applicationUserName) = CurrentUserProvider.GetCurrentApplicationUser();

        Assert.Equal(potentialDuplicate.ContactId, task.PotentialDuplicateContactId);
        Assert.Equal($"TRN request from {applicationUserName}", task.Category);
        Assert.Equal("Notification for QTS Unit Team", task.Subject);
        Assert.StartsWith($"Potential duplicate\nMatched on\n", task.Description);
    }

    private void AssertResultPersonMatchesCommand(CreateTrnRequestCommand command, TrnRequestInfoPerson person)
    {
        Assert.Equal(command.FirstName, person.FirstName);
        Assert.Equal(command.MiddleName, person.MiddleName);
        Assert.Equal(command.LastName, person.LastName);
        Assert.Equal(command.EmailAddresses.FirstOrDefault(), person.EmailAddress);
        Assert.Equal(command.DateOfBirth, person.DateOfBirth);
        Assert.Equal(command.NationalInsuranceNumber, person.NationalInsuranceNumber);
    }

    private void AssertContactMatchesCommand(
        CreateTrnRequestCommand command,
        CreateContactQuery query)
    {
        var (applicationUserId, _) = CurrentUserProvider.GetCurrentApplicationUser();

        Assert.Equal(command.FirstName, query.StatedFirstName);
        Assert.Equal(command.MiddleName, query.StatedMiddleName);
        Assert.Equal(command.LastName, query.StatedLastName);
        Assert.Equal(command.DateOfBirth, query.DateOfBirth);
        Assert.Equal(command.Gender?.ConvertToContact_GenderCode(), query.Gender);
        Assert.Equal(command.NationalInsuranceNumber, query.NationalInsuranceNumber);
        Assert.Equal(TrnRequestHelper.GetCrmTrnRequestId(applicationUserId, command.RequestId), query.TrnRequestId);
    }

    private void AssertMetadataMessageMatchesCommand(
        CreateTrnRequestCommand command,
        TrnRequestMetadataMessage message,
        bool expectedPotentialDuplicate)
    {
        var expectedApplicationUserId = CurrentUserProvider.GetCurrentApplicationUser().UserId;
        var expectedEmailAddress = command.EmailAddresses.FirstOrDefault();

        var expectedName = new[] { command.FirstName, command.MiddleName, command.LastName }
            .Where(part => !string.IsNullOrEmpty(part));

        Assert.Equal(expectedApplicationUserId, message.ApplicationUserId);
        Assert.Equal(command.RequestId, message.RequestId);
        Assert.Equal(command.IdentityVerified, message.IdentityVerified);
        Assert.Equal(expectedEmailAddress, message.EmailAddress);
        Assert.Equal(command.OneLoginUserSubject, message.OneLoginUserSubject);
        Assert.True(expectedName.SequenceEqual(message.Name));
        Assert.Equal(command.FirstName, message.FirstName);
        Assert.Equal(command.MiddleName, message.MiddleName);
        Assert.Equal(command.LastName, message.LastName);
        Assert.Equal(command.DateOfBirth, message.DateOfBirth);
        Assert.Equal(expectedPotentialDuplicate, message.PotentialDuplicate);
        Assert.Equal(command.NationalInsuranceNumber, message.NationalInsuranceNumber);
        Assert.Equal((int?)command.Gender, message.Gender);
        Assert.Null(message.AddressLine1);
        Assert.Null(message.AddressLine2);
        Assert.Null(message.AddressLine3);
        Assert.Null(message.City);
        Assert.Null(message.Postcode);
        Assert.Null(message.Country);

        if (expectedPotentialDuplicate)
        {
            Assert.Null(message.TrnToken);
        }
        else
        {
            Assert.NotNull(message.TrnToken);
        }
    }

    public async Task InitializeAsync()
    {
        // Any existing Contacts will affect our duplicate matching; clear them all out before every test
        await OperationTestFixture.DbFixture.DbHelper.ClearDataAsync();
        XrmFakedContext.DeleteAllEntities<Contact>();

        GetAnIdentityApiClientMock
            .Setup(mock => mock.CreateTrnTokenAsync(It.IsAny<CreateTrnTokenRequest>()))
            .ReturnsAsync((CreateTrnTokenRequest req) => new CreateTrnTokenResponse()
            {
                Email = req.Email,
                ExpiresUtc = Clock.UtcNow.AddDays(1),
                Trn = req.Trn,
                TrnToken = Guid.NewGuid().ToString()
            });
    }

    public Task DisposeAsync() => Task.CompletedTask;

    public enum MatchedField
    {
        FirstName,
        MiddleName,
        LastName,
        PreviousLastName,
        DateOfBirth,
        EmailAddress,
        DqtNationalInsuranceNumber,
        WorkforceNationalInsuranceNumber  // Can't support matching on this until query is done against TRS DB
    }

    public static IEnumerable<object[]> GetPotentialMatchCombinationsData()
    {
        var allMatchFields = Enum.GetValues<MatchedField>();

        var fieldGroups = new Dictionary<string, MatchedField[]>()
        {
            { "FirstName", [MatchedField.FirstName] },
            { "MiddleName", [MatchedField.MiddleName] },
            { "LastName", [MatchedField.LastName, MatchedField.PreviousLastName] },
            { "DateOfBirth", [MatchedField.DateOfBirth] },
            { "EmailAddress", [MatchedField.EmailAddress] },
            { "NationalInsuranceNumber", [MatchedField.DqtNationalInsuranceNumber, MatchedField.WorkforceNationalInsuranceNumber] }
        };

        return allMatchFields
            // Can't do potential matching against workforce data until this query is done wholly against the TRS DB
            // plus we don't have a great way to set up previous names yet
            .Except([MatchedField.WorkforceNationalInsuranceNumber, MatchedField.PreviousLastName])
            .GetCombinations()
            // Only consider combinations that include 3 distinct fields
            .Where(c => c.Select(field => fieldGroups.Single(g => g.Value.Contains(field))).Distinct().Count() >= 3)
            // DateOfBirth and Nino are considered a direct match rather than potential match
            .Where(c => !(c.Contains(MatchedField.DateOfBirth) &&
                          (c.Contains(MatchedField.DqtNationalInsuranceNumber) || c.Contains(MatchedField.WorkforceNationalInsuranceNumber))))
            .Select(c => new object[] { c });
    }
}
