using System.Net;
using TeachingRecordSystem.Api.Properties;
using TeachingRecordSystem.Api.V3.V20240307.ApiModels;
using TeachingRecordSystem.Api.V3.V20240307.Requests;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.Tests.V3.V20240307;

[Collection(nameof(DisableParallelization))]
public class CreateTrnRequestTests : TestBase
{
    public CreateTrnRequestTests(HostFixture hostFixture)
        : base(hostFixture)
    {
        SetCurrentApiClient([ApiRoles.CreateTrn]);
    }

    [Theory, RoleNamesData(except: ApiRoles.CreateTrn)]
    public async Task Post_ClientDoesNotHavePermission_ReturnsForbidden(string[] roles)
    {
        // Arrange
        SetCurrentApiClient(roles);

        var requestId = new string('x', 101);  // Limit is 100
        var requestBody = CreateJsonContent(CreateDummyRequest() with { RequestId = requestId });

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Post_RequestWithInvalidId_ReturnsError()
    {
        // Arrange
        var requestId = "$";
        var requestBody = CreateJsonContent(CreateDummyRequest() with { RequestId = requestId });

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: nameof(CreateTrnRequestRequest.RequestId),
            expectedError: StringResources.ErrorMessages_RequestIdCanOnlyContainCharactersDigitsUnderscoresAndDashes);
    }

    [Fact]
    public async Task Post_RequestIdExceedingCharacterLimit_ReturnsError()
    {
        // Arrange
        var requestId = new string('x', 101);  // Limit is 100
        var requestBody = CreateJsonContent(CreateDummyRequest() with { RequestId = requestId });

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: nameof(CreateTrnRequestRequest.RequestId),
            expectedError: StringResources.ErrorMessages_RequestIdMustBe100CharactersOrFewer);
    }

    [Theory]
    [InlineData(1900, 1, 1)]
    public async Task Post_DateOfBirthBefore01011940_ReturnsError(int year, int month, int day)
    {
        // Arrange
        var requestBody = CreateJsonContent(CreateDummyRequest() with
        {
            Person = CreateDummyRequestPerson() with { DateOfBirth = new DateOnly(year, month, day) }
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            "Person.DateOfBirth",
            StringResources.ErrorMessages_BirthDateIsOutOfRange);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task Post_RequestWithDateOfBirthEqualOrAfterToday_ReturnsError(int daysAfterToday)
    {
        // Arrange
        var dateOfBirth = Clock.Today.AddDays(daysAfterToday);

        var requestBody = CreateJsonContent(CreateDummyRequest() with
        {
            Person = CreateDummyRequestPerson() with { DateOfBirth = dateOfBirth }
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            "Person.DateOfBirth",
            StringResources.ErrorMessages_BirthDateIsOutOfRange);
    }

    [Fact]
    public async Task Post_NationalInsuranceNumberExceedingMaxLength_ReturnsError()
    {
        // Arrange
        var nationalInsuranceNumber = new string('x', 10);

        var requestBody = CreateJsonContent(CreateDummyRequest() with
        {
            Person = CreateDummyRequestPerson() with { NationalInsuranceNumber = nationalInsuranceNumber }
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            "Person.NationalInsuranceNumber",
            StringResources.ErrorMessages_EnterNinoNumberInCorrectFormat);
    }

    [Fact]
    public async Task Post_RequestWithExistingRequestInCrm_ReturnsConflict()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();
        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn()
            .WithFirstName(firstName)
            .WithMiddleName(middleName)
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(email)
            .WithNationalInsuranceNumber(nationalInsuranceNumber: nationalInsuranceNumber));

        await WithDbContext(async dbContext =>
        {
            dbContext.Add(new TrnRequest()
            {
                ClientId = ClientId,
                RequestId = requestId,
                TeacherId = existingContact.ContactId
            });

            await dbContext.SaveChangesAsync();
        });

        var requestBody = CreateJsonContent(CreateDummyRequest() with
        {
            RequestId = requestId,
            Person = new()
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                Email = email,
                NationalInsuranceNumber = nationalInsuranceNumber,
            }
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseIsError(response, expectedErrorCode: 10029, expectedStatusCode: StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task Post_RequestWithExistingRequestInDb_ReturnsConflict()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();
        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn()
            .WithFirstName(firstName)
            .WithMiddleName(middleName)
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(email)
            .WithNationalInsuranceNumber(nationalInsuranceNumber: nationalInsuranceNumber)
            .WithTrnRequestId(TrnRequestHelper.GetCrmTrnRequestId(ClientId, requestId)));

        var requestBody = CreateJsonContent(CreateDummyRequest() with
        {
            RequestId = requestId,
            Person = new()
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                Email = email,
                NationalInsuranceNumber = nationalInsuranceNumber,
            }
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseIsError(response, expectedErrorCode: 10029, expectedStatusCode: StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task Post_NotMatchedToExistingRecord_CreatesTeacherWithTrnAndReturnsCompletedStatus()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var firstNames = new string[] { Faker.Name.First(), Faker.Name.First() };
        var firstName = string.Join(" ", firstNames);
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();
        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

        var requestBody = CreateJsonContent(CreateDummyRequest() with
        {
            RequestId = requestId,
            Person = new()
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                Email = email,
                NationalInsuranceNumber = nationalInsuranceNumber,
            }
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var (_, createdContactId) = CrmQueryDispatcherSpy.GetSingleQuery<CreateContactQuery, Guid>();
        var contact = XrmFakedContext.CreateQuery<Contact>().SingleOrDefault(c => c.Id == createdContactId);
        Assert.NotNull(contact);
        Assert.NotEmpty(contact.dfeta_TRN);
        Assert.Equal(dateOfBirth.ToDateTimeWithDqtBstFix(isLocalTime: false), contact.BirthDate);
        Assert.Equal(firstNames.First(), contact.FirstName);
        Assert.Equal(string.Join(" ", firstNames.Skip(1).Append(middleName)), contact.MiddleName);
        Assert.Equal(lastName, contact.LastName);
        Assert.Equal(firstName, contact.dfeta_StatedFirstName);
        Assert.Equal(middleName, contact.dfeta_StatedMiddleName);
        Assert.Equal(lastName, contact.dfeta_StatedLastName);
        Assert.Equal(email, contact.EMailAddress1);
        Assert.Equal(nationalInsuranceNumber, contact.dfeta_NINumber);

        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                requestId,
                person = new
                {
                    firstName,
                    middleName,
                    lastName,
                    email,
                    dateOfBirth,
                    nationalInsuranceNumber,
                },
                trn = contact.dfeta_TRN,
                status = "Completed"
            },
            expectedStatusCode: 200);
    }

    [Fact]
    public async Task Post_RequestWithInvalidNino_ReturnsError()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();
        var invalidNino = "IvalidNi";

        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn()
            .WithFirstName(firstName)
            .WithMiddleName(middleName)
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(email)
            .WithNationalInsuranceNumber());

        await WithDbContext(async dbContext =>
        {
            dbContext.Add(new TrnRequest()
            {
                ClientId = ClientId,
                RequestId = requestId,
                TeacherId = existingContact.ContactId
            });

            await dbContext.SaveChangesAsync();
        });

        var requestBody = CreateJsonContent(CreateDummyRequest() with
        {
            RequestId = requestId,
            Person = new()
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                Email = email,
                NationalInsuranceNumber = invalidNino,
            }
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            "Person.NationalInsuranceNumber",
            StringResources.ErrorMessages_EnterNinoNumberInCorrectFormat);
    }

    [Fact]
    public async Task Post_RequestWithoutNino_ReturnsOk()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();

        var requestBody = CreateJsonContent(CreateDummyRequest() with
        {
            RequestId = requestId,
            Person = new()
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                Email = email
            }
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Post_RequestWithoutEmail_ReturnsOk()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);

        var requestBody = CreateJsonContent(CreateDummyRequest() with
        {
            RequestId = requestId,
            Person = new()
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                DateOfBirth = dateOfBirth
            }
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Post_PotentialDuplicateContactOnNamesAndDateOfBirth_CreatesContactWithoutTrnAndReturnsPendingStatus()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();
        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

        await TestData.CreatePerson(p => p
            .WithTrn()
            .WithFirstName(firstName)
            .WithMiddleName(middleName)
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth));

        var requestBody = CreateJsonContent(CreateDummyRequest() with
        {
            RequestId = requestId,
            Person = new()
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                Email = email,
                NationalInsuranceNumber = nationalInsuranceNumber,
            }
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var (_, createdContactId) = CrmQueryDispatcherSpy.GetSingleQuery<CreateContactQuery, Guid>();
        var contact = XrmFakedContext.CreateQuery<Contact>().SingleOrDefault(c => c.Id == createdContactId);
        Assert.NotNull(contact);
        Assert.Null(contact.dfeta_TRN);

        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                requestId,
                person = new
                {
                    firstName,
                    middleName,
                    lastName,
                    email,
                    dateOfBirth,
                    nationalInsuranceNumber,
                },
                trn = (string?)null,
                status = "Pending"
            },
            expectedStatusCode: 200);
    }

    [Fact]
    public async Task Post_PotentialDuplicateContactMatchedOnDqtNinoOnly_CreatesContactWithoutTrnAndReturnsPendingStatus()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();
        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

        await TestData.CreatePerson(p => p
            .WithTrn()
            .WithFirstName(TestData.GenerateChangedFirstName(firstName))
            .WithMiddleName(TestData.GenerateChangedMiddleName(middleName))
            .WithLastName(TestData.GenerateChangedLastName(lastName))
            .WithDateOfBirth(TestData.GenerateChangedDateOfBirth(dateOfBirth))
            .WithNationalInsuranceNumber(nationalInsuranceNumber));

        var requestBody = CreateJsonContent(CreateDummyRequest() with
        {
            RequestId = requestId,
            Person = new()
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                Email = email,
                NationalInsuranceNumber = nationalInsuranceNumber,
            }
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var (_, createdContactId) = CrmQueryDispatcherSpy.GetSingleQuery<CreateContactQuery, Guid>();
        var contact = XrmFakedContext.CreateQuery<Contact>().SingleOrDefault(c => c.Id == createdContactId);
        Assert.NotNull(contact);
        Assert.Null(contact.dfeta_TRN);

        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                requestId,
                person = new
                {
                    firstName,
                    middleName,
                    lastName,
                    email,
                    dateOfBirth,
                    nationalInsuranceNumber,
                },
                trn = (string?)null,
                status = "Pending"
            },
            expectedStatusCode: 200);
    }

    [Fact]
    public async Task Post_PotentialDuplicateContactMatchedOnWorkforceDataNinoOnly_CreatesContactWithoutTrnAndReturnsPendingStatus()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();
        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

        var person = await TestData.CreatePerson(p => p
            .WithTrn()
            .WithFirstName(TestData.GenerateChangedFirstName(firstName))
            .WithMiddleName(TestData.GenerateChangedMiddleName(middleName))
            .WithLastName(TestData.GenerateChangedLastName(lastName))
            .WithDateOfBirth(TestData.GenerateChangedDateOfBirth(dateOfBirth)));

        var establishment = await TestData.CreateEstablishment(localAuthorityCode: "321");
        await TestData.CreateTpsEmployment(
            person,
            establishment,
            startDate: new DateOnly(2024, 1, 1),
            lastKnownEmployedDate: new DateOnly(2024, 10, 1),
            EmploymentType.FullTime,
            lastExtractDate: new DateOnly(2024, 10, 1),
            nationalInsuranceNumber: nationalInsuranceNumber);

        var requestBody = CreateJsonContent(CreateDummyRequest() with
        {
            RequestId = requestId,
            Person = new()
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                Email = email,
                NationalInsuranceNumber = nationalInsuranceNumber,
            }
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var (_, createdContactId) = CrmQueryDispatcherSpy.GetSingleQuery<CreateContactQuery, Guid>();
        var contact = XrmFakedContext.CreateQuery<Contact>().SingleOrDefault(c => c.Id == createdContactId);
        Assert.NotNull(contact);
        Assert.Null(contact.dfeta_TRN);

        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                requestId,
                person = new
                {
                    firstName,
                    middleName,
                    lastName,
                    email,
                    dateOfBirth,
                    nationalInsuranceNumber,
                },
                trn = (string?)null,
                status = "Pending"
            },
            expectedStatusCode: 200);
    }

    [Fact]
    public async Task Post_DuplicateContactMatchedOnDqtNinoAndDateOfBirth_ReturnsExistingTrnAndDoesNotCreateNewContact()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();
        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

        var person = await TestData.CreatePerson(p => p
            .WithTrn()
            .WithFirstName(TestData.GenerateChangedFirstName(firstName))
            .WithMiddleName(TestData.GenerateChangedMiddleName(middleName))
            .WithLastName(TestData.GenerateChangedLastName(lastName))
            .WithDateOfBirth(dateOfBirth)
            .WithNationalInsuranceNumber(nationalInsuranceNumber));

        var requestBody = CreateJsonContent(CreateDummyRequest() with
        {
            RequestId = requestId,
            Person = new()
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                Email = email,
                NationalInsuranceNumber = nationalInsuranceNumber,
            }
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Empty(CrmQueryDispatcherSpy.GetAllQueries<CreateContactQuery, Guid>());

        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                requestId,
                person = new
                {
                    firstName = person.FirstName,
                    middleName = person.MiddleName,
                    lastName = person.LastName,
                    email = person.Email,
                    dateOfBirth = person.DateOfBirth,
                    nationalInsuranceNumber = person.NationalInsuranceNumber,
                },
                trn = person.Trn,
                status = "Completed"
            },
            expectedStatusCode: 200);
    }

    [Fact]
    public async Task Post_DuplicateContactMatchedOnWorkforceDataNinoAndDqtDateOfBirth_ReturnsExistingTrnAndDoesNotCreateNewContact()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();
        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

        var person = await TestData.CreatePerson(p => p
            .WithTrn()
            .WithFirstName(TestData.GenerateChangedFirstName(firstName))
            .WithMiddleName(TestData.GenerateChangedMiddleName(middleName))
            .WithLastName(TestData.GenerateChangedLastName(lastName))
            .WithDateOfBirth(dateOfBirth));

        var establishment = await TestData.CreateEstablishment(localAuthorityCode: "321");
        await TestData.CreateTpsEmployment(
            person,
            establishment,
            startDate: new DateOnly(2024, 1, 1),
            lastKnownEmployedDate: new DateOnly(2024, 10, 1),
            EmploymentType.FullTime,
            lastExtractDate: new DateOnly(2024, 10, 1),
            nationalInsuranceNumber: nationalInsuranceNumber);

        var requestBody = CreateJsonContent(CreateDummyRequest() with
        {
            RequestId = requestId,
            Person = new()
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                Email = email,
                NationalInsuranceNumber = nationalInsuranceNumber,
            }
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Empty(CrmQueryDispatcherSpy.GetAllQueries<CreateContactQuery, Guid>());

        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                requestId,
                person = new
                {
                    firstName = person.FirstName,
                    middleName = person.MiddleName,
                    lastName = person.LastName,
                    email = person.Email,
                    dateOfBirth = person.DateOfBirth,
                    nationalInsuranceNumber = person.NationalInsuranceNumber,
                },
                trn = person.Trn,
                status = "Completed"
            },
            expectedStatusCode: 200);
    }

    private static CreateTrnRequestRequest CreateDummyRequest() => new()
    {
        RequestId = Guid.NewGuid().ToString(),
        Person = CreateDummyRequestPerson()
    };

    private static TrnRequestPerson CreateDummyRequestPerson() => new()
    {
        FirstName = "Minnie",
        MiddleName = "Van",
        LastName = "Ryder",
        DateOfBirth = new(1990, 5, 23),
        Email = "minnie.van.ryder@example.com",
        NationalInsuranceNumber = "1234567D"
    };
}
