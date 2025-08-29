using System.Diagnostics;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models;
using TeachingRecordSystem.Core.Services.TrnRequests;

#pragma warning disable TRS0001

namespace TeachingRecordSystem.Api.UnitTests.V3;

[Collection(nameof(DisableParallelization))]
public class CreateTrnRequestTests : OperationTestBase, IAsyncLifetime
{
    public CreateTrnRequestTests(OperationTestFixture operationTestFixture) : base(operationTestFixture)
    {
    }

    [Fact]
    public Task HandleAsync_RequestForSameUserAndIdAlreadyExists_ReturnsError() =>
        WithHandler<CreateTrnRequestHandler>(async handler =>
        {
            // Arrange
            var command = CreateCommand();
            var applicationUserId = CurrentUserProvider.GetCurrentApplicationUser().UserId;

            await TestData.CreatePersonAsync(p => p
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
            var metadata = await DbFixture.WithDbContextAsync(dbContext =>
                dbContext.TrnRequestMetadata
                    .SingleAsync(m =>
                        m.ApplicationUserId == CurrentUserProvider.GetCurrentApplicationUser().UserId && m.RequestId == command.RequestId));
            Assert.Equal(expectedNormalizedInsuranceNumber, metadata.NationalInsuranceNumber);
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
    public Task HandleAsync_MatchingExistingPersonOnTwoNamesAndDateOfBirth_ReturnsPendingStatusAndCreatesSupportTask(MatchedField[] matchedFields) =>
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
                .WithFirstName(matchedFields.Contains(MatchedField.FirstName) ? firstName : TestData.GenerateChangedFirstName(firstName))
                .WithMiddleName(matchedFields.Contains(MatchedField.MiddleName) ? middleName : TestData.GenerateChangedMiddleName(middleName))
                .WithLastName(matchedFields.Contains(MatchedField.LastName) ? lastName : TestData.GenerateChangedLastName(lastName))
                .WithDateOfBirth(matchedFields.Contains(MatchedField.DateOfBirth) ? dateOfBirth : TestData.GenerateChangedDateOfBirth(dateOfBirth))
                .WithEmail(matchedFields.Contains(MatchedField.EmailAddress) ? emailAddress : TestData.GenerateUniqueEmail())
                .WithNationalInsuranceNumber(matchedFields.Contains(MatchedField.TrsNationalInsuranceNumber)
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

            var persons = await DbFixture.WithDbContextAsync(dbContext => dbContext.Persons.Where(p => p.PersonId != matchedPerson.PersonId).ToArrayAsync());
            Assert.Empty(persons);

            await AssertSupportTaskCreatedAsync(CurrentUserProvider.GetCurrentApplicationUser().UserId, command.RequestId);

            await AssertMetadataMatchesCommandAsync(command, expectedPotentialDuplicate: true);
        });

    [Fact]
    public Task HandleAsync_MatchingExistingPersonOnTrsNinoAndDob_ReturnsTrnOfExistingPersonDoesNotCreatePersonOrSupportTask() =>
        WithHandler<CreateTrnRequestHandler>(async handler =>
        {
            // Arrange
            var dateOfBirth = new DateOnly(1990, 01, 01);
            var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

            var matchedPerson = await TestData.CreatePersonAsync(p => p
                .WithDateOfBirth(dateOfBirth)
                .WithNationalInsuranceNumber(nationalInsuranceNumber));

            var command = CreateCommand() with
            {
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

            var persons = await DbFixture.WithDbContextAsync(dbContext => dbContext.Persons.Where(p => p.PersonId != matchedPerson.PersonId).ToArrayAsync());
            Assert.Empty(persons);

            await AssertMetadataMatchesCommandAsync(command, expectedPotentialDuplicate: false);

            await AssertNoSupportTaskCreatedAsync(CurrentUserProvider.GetCurrentApplicationUser().UserId, command.RequestId);
        });

    [Fact]
    public Task HandleAsync_MatchingExistingPersonOnTrsNinoOnly_CreatesSupportTask() =>
        WithHandler<CreateTrnRequestHandler>(async handler =>
        {
            // Arrange
            var dateOfBirth = new DateOnly(1990, 01, 01);
            var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

            var matchedPerson = await TestData.CreatePersonAsync(p => p
                .WithDateOfBirth(TestData.GenerateChangedDateOfBirth(dateOfBirth))
                .WithNationalInsuranceNumber(nationalInsuranceNumber));

            var command = CreateCommand() with
            {
                DateOfBirth = dateOfBirth,
                NationalInsuranceNumber = nationalInsuranceNumber
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

            var persons = await DbFixture.WithDbContextAsync(dbContext =>
                dbContext.Persons.Where(p => p.PersonId != matchedPerson.PersonId).ToArrayAsync());
            Assert.Empty(persons);

            await AssertSupportTaskCreatedAsync(CurrentUserProvider.GetCurrentApplicationUser().UserId, command.RequestId);

            await AssertMetadataMatchesCommandAsync(command, expectedPotentialDuplicate: true);
        });

    [Fact]
    public Task HandleAsync_MatchingExistingPersonOnEmailOnly_CreatesSupportTask() =>
        WithHandler<CreateTrnRequestHandler>(async handler =>
        {
            // Arrange
            var emailAddress = TestData.GenerateUniqueEmail();

            var matchedPerson = await TestData.CreatePersonAsync(p => p.WithEmail(emailAddress));

            var command = CreateCommand() with
            {
                EmailAddresses = [emailAddress]
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

            var persons = await DbFixture.WithDbContextAsync(dbContext =>
                dbContext.Persons.Where(p => p.PersonId != matchedPerson.PersonId).ToArrayAsync());
            Assert.Empty(persons);

            await AssertSupportTaskCreatedAsync(CurrentUserProvider.GetCurrentApplicationUser().UserId, command.RequestId);

            await AssertMetadataMatchesCommandAsync(command, expectedPotentialDuplicate: true);
        });

    [Fact]
    public Task HandleAsync_MatchingMultipleExistingPersonsOnTrsNinoAndDob_CreatesSupportTask() =>
        WithHandler<CreateTrnRequestHandler>(async handler =>
        {
            // Arrange
            var dateOfBirth = new DateOnly(1990, 01, 01);
            var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

            var matchedPerson1 = await TestData.CreatePersonAsync(p => p
                .WithDateOfBirth(dateOfBirth)
                .WithNationalInsuranceNumber(nationalInsuranceNumber));

            var matchedPerson2 = await TestData.CreatePersonAsync(p => p
                .WithDateOfBirth(dateOfBirth)
                .WithNationalInsuranceNumber(nationalInsuranceNumber));

            var command = CreateCommand() with
            {
                DateOfBirth = dateOfBirth,
                NationalInsuranceNumber = nationalInsuranceNumber
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

            var persons = await DbFixture.WithDbContextAsync(dbContext =>
                dbContext.Persons.Where(p => p.PersonId != matchedPerson1.PersonId && p.PersonId != matchedPerson2.PersonId).ToArrayAsync());
            Assert.Empty(persons);

            await AssertSupportTaskCreatedAsync(CurrentUserProvider.GetCurrentApplicationUser().UserId, command.RequestId);

            await AssertMetadataMatchesCommandAsync(command, expectedPotentialDuplicate: true);
        });

    [Fact]
    public Task HandleAsync_MatchingExistingPersonOnWorkforceNinoAndDob_ReturnsTrnOfExistingPersonDoesNotCreatePerson() =>
        WithHandler<CreateTrnRequestHandler>(async handler =>
        {
            // Arrange
            var dateOfBirth = new DateOnly(1990, 01, 01);
            var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

            var matchedPerson = await TestData.CreatePersonAsync(p => p
                .WithDateOfBirth(dateOfBirth));
            Debug.Assert(matchedPerson.NationalInsuranceNumber is null);

            var establishment = await TestData.CreateEstablishmentAsync(localAuthorityCode: "321");
            await TestData.CreateTpsEmploymentAsync(
                matchedPerson,
                establishment,
                startDate: new DateOnly(2024, 1, 1),
                lastKnownEmployedDate: new DateOnly(2024, 10, 1),
                EmploymentType.FullTime,
                lastExtractDate: new DateOnly(2024, 10, 1),
                nationalInsuranceNumber: nationalInsuranceNumber);

            var command = CreateCommand() with
            {
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

            var persons = await DbFixture.WithDbContextAsync(dbContext => dbContext.Persons.Where(p => p.PersonId != matchedPerson.PersonId).ToArrayAsync());
            Assert.Empty(persons);

            await AssertNoSupportTaskCreatedAsync(CurrentUserProvider.GetCurrentApplicationUser().UserId, command.RequestId);

            await AssertMetadataMatchesCommandAsync(command, expectedPotentialDuplicate: false);
        });

    [Fact]
    public Task HandleAsync_DefiniteMatchWithPersonDoesNotRequireFurthersChecks_ReturnsTrn() =>
        WithHandler<CreateTrnRequestHandler>(async handler =>
        {
            // Arrange
            TrnRequestOptions.FlagFurtherChecksRequiredFromUserIds = [
                CurrentUserProvider.GetCurrentApplicationUser().UserId
            ];

            var dateOfBirth = new DateOnly(1990, 01, 01);
            var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

            var matchedPerson = await TestData.CreatePersonAsync(p => p
                .WithDateOfBirth(dateOfBirth)
                .WithNationalInsuranceNumber(nationalInsuranceNumber));
            Debug.Assert(matchedPerson.Alerts.Count == 0 && matchedPerson.QtsDate is null && matchedPerson.EytsDate is null);

            var command = CreateCommand() with
            {
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

            await DbFixture.WithDbContextAsync(async dbContext =>
            {
                var metadata = await dbContext.TrnRequestMetadata
                    .SingleAsync(m => m.ApplicationUserId == CurrentUserProvider.GetCurrentApplicationUser().UserId && m.RequestId == command.RequestId);
                Assert.Equal(TrnRequestStatus.Completed, metadata.Status);
                Assert.Equal(matchedPerson.PersonId, metadata.ResolvedPersonId);

                var supportTasks = await dbContext.SupportTasks
                    .Where(t => t.SupportTaskType == SupportTaskType.TrnRequestManualChecksNeeded &&
                        t.TrnRequestMetadata!.ApplicationUserId == CurrentUserProvider.GetCurrentApplicationUser().UserId &&
                        t.TrnRequestMetadata!.RequestId == command.RequestId)
                    .ToArrayAsync();

                Assert.Empty(supportTasks);
            });
        });

    [Fact]
    public Task HandleAsync_DefiniteMatchWithPersonDoesRequireFurthersChecks_CreatesSupportTaskAndDoesNotReturnTrn() =>
        WithHandler<CreateTrnRequestHandler>(async handler =>
        {
            // Arrange
            TrnRequestOptions.FlagFurtherChecksRequiredFromUserIds = [
                CurrentUserProvider.GetCurrentApplicationUser().UserId
            ];

            var dateOfBirth = new DateOnly(1990, 01, 01);
            var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

            var matchedPerson = await TestData.CreatePersonAsync(p => p
                .WithDateOfBirth(dateOfBirth)
                .WithNationalInsuranceNumber(nationalInsuranceNumber)
                .WithAlert());

            var command = CreateCommand() with
            {
                DateOfBirth = dateOfBirth,
                NationalInsuranceNumber = nationalInsuranceNumber
            };

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            var success = AssertSuccess(result);

            Assert.Equal(command.RequestId, success.RequestId);
            Assert.Equal(TrnRequestStatus.Pending, success.Status);
            Assert.Null(success.Trn);
            Assert.Null(success.AccessYourTeachingQualificationsLink);

            await DbFixture.WithDbContextAsync(async dbContext =>
            {
                var metadata = await dbContext.TrnRequestMetadata
                    .SingleAsync(m => m.ApplicationUserId == CurrentUserProvider.GetCurrentApplicationUser().UserId && m.RequestId == command.RequestId);
                Assert.Equal(TrnRequestStatus.Pending, metadata.Status);
                Assert.Equal(matchedPerson.PersonId, metadata.ResolvedPersonId);

                var supportTasks = await dbContext.SupportTasks
                    .Where(t => t.SupportTaskType == SupportTaskType.TrnRequestManualChecksNeeded &&
                        t.TrnRequestMetadata!.ApplicationUserId == CurrentUserProvider.GetCurrentApplicationUser().UserId &&
                        t.TrnRequestMetadata!.RequestId == command.RequestId)
                    .ToArrayAsync();

                Assert.NotEmpty(supportTasks);
            });
        });

    [Fact]
    public Task HandleAsync_NoMatches_CreatesPersonWithTrnButNoSupportTask() =>
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

            var person = await DbFixture.WithDbContextAsync(dbContext => dbContext.Persons.SingleOrDefaultAsync());
            Assert.NotNull(person);
            Assert.NotNull(person.Trn);
            AssertPersonMatchesCommand(command, person, expectTrn: true);

            await AssertNoSupportTaskCreatedAsync(CurrentUserProvider.GetCurrentApplicationUser().UserId, command.RequestId);

            await AssertMetadataMatchesCommandAsync(command, expectedPotentialDuplicate: false);
        });

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

    private void AssertResultPersonMatchesCommand(CreateTrnRequestCommand command, TrnRequestInfoPerson person)
    {
        Assert.Equal(command.FirstName, person.FirstName);
        Assert.Equal(command.MiddleName, person.MiddleName);
        Assert.Equal(command.LastName, person.LastName);
        Assert.Equal(command.EmailAddresses.FirstOrDefault(), person.EmailAddress);
        Assert.Equal(command.DateOfBirth, person.DateOfBirth);
        Assert.Equal(command.NationalInsuranceNumber, person.NationalInsuranceNumber);
    }

    private void AssertPersonMatchesCommand(
        CreateTrnRequestCommand command,
        Person person,
        bool expectTrn)
    {
        Assert.Equal(command.FirstName, person.FirstName);
        Assert.Equal(command.MiddleName, person.MiddleName);
        Assert.Equal(command.LastName, person.LastName);
        Assert.Equal(command.DateOfBirth, person.DateOfBirth);
        Assert.Equal(command.Gender, person.Gender);
        Assert.Equal(command.NationalInsuranceNumber, person.NationalInsuranceNumber);

        if (expectTrn)
        {
            Assert.NotNull(person.Trn);
        }
        else
        {
            Assert.Null(person.Trn);
        }
    }

    private Task AssertMetadataMatchesCommandAsync(CreateTrnRequestCommand command, bool expectedPotentialDuplicate) =>
        DbFixture.WithDbContextAsync(async dbContext =>
        {
            var applicationUserId = CurrentUserProvider.GetCurrentApplicationUser().UserId;

            var metadata = await dbContext.TrnRequestMetadata
                .SingleOrDefaultAsync(m => m.ApplicationUserId == applicationUserId && m.RequestId == command.RequestId);
            Assert.NotNull(metadata);

            var expectedEmailAddress = command.EmailAddresses.FirstOrDefault();

            var expectedName = new[] { command.FirstName, command.MiddleName, command.LastName }
                .Where(part => !string.IsNullOrEmpty(part));

            Assert.Equal(command.IdentityVerified, metadata.IdentityVerified);
            Assert.Equal(expectedEmailAddress, metadata.EmailAddress);
            Assert.Equal(command.OneLoginUserSubject, metadata.OneLoginUserSubject);
            Assert.True(expectedName.SequenceEqual(metadata.Name));
            Assert.Equal(command.FirstName, metadata.FirstName);
            Assert.Equal(command.MiddleName, metadata.MiddleName);
            Assert.Equal(command.LastName, metadata.LastName);
            Assert.Equal(command.DateOfBirth, metadata.DateOfBirth);
            Assert.Equal(expectedPotentialDuplicate, metadata.PotentialDuplicate);
            Assert.Equal(command.NationalInsuranceNumber, metadata.NationalInsuranceNumber);
            Assert.Equal(command.Gender, metadata.Gender);
            Assert.Null(metadata.AddressLine1);
            Assert.Null(metadata.AddressLine2);
            Assert.Null(metadata.AddressLine3);
            Assert.Null(metadata.City);
            Assert.Null(metadata.Postcode);
            Assert.Null(metadata.Country);

            if (expectedPotentialDuplicate)
            {
                Assert.Null(metadata.TrnToken);
            }
            else
            {
                Assert.NotNull(metadata.TrnToken);
            }
        });

    private Task AssertSupportTaskCreatedAsync(Guid applicationUserId, string requestId) =>
        DbFixture.WithDbContextAsync(async dbContext =>
        {
            var supportTask = await dbContext.SupportTasks
                .SingleOrDefaultAsync(t => t.SupportTaskType == SupportTaskType.ApiTrnRequest &&
                    t.TrnRequestMetadata!.ApplicationUserId == applicationUserId &&
                    t.TrnRequestMetadata!.RequestId == requestId);

            Assert.NotNull(supportTask);
            Assert.Equal(SupportTaskStatus.Open, supportTask.Status);
        });

    private Task AssertNoSupportTaskCreatedAsync(Guid applicationUserId, string requestId) =>
        DbFixture.WithDbContextAsync(async dbContext =>
        {
            var supportTask = await dbContext.SupportTasks
                .SingleOrDefaultAsync(t => t.SupportTaskType == SupportTaskType.ApiTrnRequest &&
                    t.TrnRequestMetadata!.ApplicationUserId == applicationUserId &&
                    t.TrnRequestMetadata!.RequestId == requestId);

            Assert.Null(supportTask);
        });

    public async Task InitializeAsync()
    {
        // Any existing Contacts will affect our duplicate matching; clear them all out before every test
        await OperationTestFixture.DbFixture.DbHelper.DeleteAllPersonsAsync();

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
        TrsNationalInsuranceNumber,
        WorkforceNationalInsuranceNumber
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
            { "NationalInsuranceNumber", [MatchedField.TrsNationalInsuranceNumber, MatchedField.WorkforceNationalInsuranceNumber] }
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
                          (c.Contains(MatchedField.TrsNationalInsuranceNumber) || c.Contains(MatchedField.WorkforceNationalInsuranceNumber))))
            .Select(c => new object[] { c });
    }
}
