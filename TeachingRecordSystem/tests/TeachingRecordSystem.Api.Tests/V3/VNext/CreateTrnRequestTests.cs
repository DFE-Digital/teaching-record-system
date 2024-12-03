using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using TeachingRecordSystem.Api.V3.VNext.Requests;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Services.DqtOutbox;
using TeachingRecordSystem.Core.Services.DqtOutbox.Messages;

namespace TeachingRecordSystem.Api.Tests.V3.VNext;

[Collection(nameof(DisableParallelization))]
public class CreateTrnRequestTests : TestBase
{
    public CreateTrnRequestTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentApiClient([ApiRoles.CreateTrn]);
        XrmFakedContext.DeleteAllEntities<Contact>();
    }

    [Fact]
    public async Task Post_CreatesOutboxMessageInCrm()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var firstName = TestData.GenerateFirstName();
        var middleName = TestData.GenerateMiddleName();
        var lastName = TestData.GenerateLastName();
        var dateOfBirth = TestData.GenerateDateOfBirth();
        var email = TestData.GenerateUniqueEmail();
        var identityVerified = true;
        var oneLoginUserSubject = TestData.CreateOneLoginUserSubject();

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = CreateJsonContent(new
            {
                requestId = requestId,
                person = new
                {
                    firstName = firstName,
                    middleName = middleName,
                    lastName = lastName,
                    dateOfBirth = dateOfBirth,
                    emailAddresses = new[] { email }
                },
                identityVerified = identityVerified,
                oneLoginUserSubject = oneLoginUserSubject
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response, expectedStatusCode: StatusCodes.Status200OK);

        var (crmQuery, _) = CrmQueryDispatcherSpy.GetSingleQuery<CreateContactQuery, Guid>();
        Assert.Collection(
            crmQuery.OutboxMessages,
            outboxMessage =>
            {
                Assert.Equal(nameof(TrnRequestMetadataMessage), outboxMessage.dfeta_MessageName);

                var messageSerializer = HostFixture.Services.GetRequiredService<MessageSerializer>();
                var message = Assert.IsType<TrnRequestMetadataMessage>(messageSerializer.DeserializeMessage(outboxMessage.dfeta_Payload, outboxMessage.dfeta_MessageName));
                Assert.Equal(ApplicationUserId, message.ApplicationUserId);
                Assert.Equal(requestId, message.RequestId);
                Assert.Equal(Clock.UtcNow, message.CreatedOn);
                Assert.Equal(email, message.EmailAddress);
                Assert.Equal(identityVerified, message.IdentityVerified);
                Assert.Equal(oneLoginUserSubject, message.OneLoginUserSubject);
                Assert.Equal(new[] { firstName, middleName, lastName }, message.Name);
                Assert.Equal(dateOfBirth, message.DateOfBirth);
            });
    }

    [Fact]
    public async Task Post_ValidAddressFields_PopulatesContactAddressFields()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var firstName = TestData.GenerateFirstName();
        var middleName = TestData.GenerateMiddleName();
        var lastName = TestData.GenerateLastName();
        var dateOfBirth = TestData.GenerateDateOfBirth();
        var email = TestData.GenerateUniqueEmail();
        var identityVerified = true;
        var oneLoginUserSubject = TestData.CreateOneLoginUserSubject();
        var addressLine1 = Faker.Address.StreetName();
        var addressLine2 = Faker.Address.StreetName();
        var addressLine3 = Faker.Address.StreetName();
        var postcode = Faker.Address.UkPostCode();
        var country = Faker.Address.Country();
        var gender = Gender.Female;
        var city = Faker.Address.City();

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = CreateJsonContent(new
            {
                requestId = requestId,
                person = new
                {
                    firstName = firstName,
                    middleName = middleName,
                    lastName = lastName,
                    dateOfBirth = dateOfBirth,
                    emailAddresses = new[] { email },
                    address = new
                    {
                        addressLine1 = addressLine1,
                        addressLine2 = addressLine2,
                        addressLine3 = addressLine3,
                        city = city,
                        postcode = postcode,
                        country = country,
                    },
                    genderCode = gender
                },
                identityVerified = identityVerified,
                oneLoginUserSubject = oneLoginUserSubject
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseAsync(response, expectedStatusCode: StatusCodes.Status200OK);

        var (crmQuery, _) = CrmQueryDispatcherSpy.GetSingleQuery<CreateContactQuery, Guid>();
        Assert.Equal(addressLine1, crmQuery.Address1Line1);
        Assert.Equal(addressLine2, crmQuery.Address1Line2);
        Assert.Equal(addressLine3, crmQuery.Address1Line3);
        Assert.Equal(city, crmQuery.Address1City);
        Assert.Equal(postcode, crmQuery.Address1PostalCode);
        Assert.Equal(country, crmQuery.Address1Country);
        Assert.Equal(Contact_GenderCode.Female, crmQuery.Gender);
    }

    [Fact]
    public async Task Post_AddressFieldsExceedingMaxLengths_ReturnsBadRequest()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var firstName = TestData.GenerateFirstName();
        var middleName = TestData.GenerateMiddleName();
        var lastName = TestData.GenerateLastName();
        var dateOfBirth = TestData.GenerateDateOfBirth();
        var email = TestData.GenerateUniqueEmail();
        var identityVerified = true;
        var oneLoginUserSubject = TestData.CreateOneLoginUserSubject();
        var addressLine1 = new string('x', 255);
        var addressLine2 = new string('x', 255);
        var addressLine3 = new string('x', 255);
        var postcode = new string('x', 255);
        var country = new string('x', 255);
        var gender = Gender.Female;
        var city = new string('x', 255);

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = CreateJsonContent(new
            {
                requestId = requestId,
                person = new
                {
                    firstName = firstName,
                    middleName = middleName,
                    lastName = lastName,
                    dateOfBirth = dateOfBirth,
                    emailAddresses = new[] { email },
                    address = new
                    {
                        addressLine1 = addressLine1,
                        addressLine2 = addressLine2,
                        addressLine3 = addressLine3,
                        city = city,
                        postcode = postcode,
                        country = country,
                    },
                    genderCode = gender
                },
                identityVerified = identityVerified,
                oneLoginUserSubject = oneLoginUserSubject
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorsForPropertiesAsync(
            response,
            new Dictionary<string, string>()
            {
                { $"{nameof(CreateTrnRequestRequest.Person)}.{nameof(CreateTrnRequestRequestPerson.Address)}.{nameof(CreateTrnRequestAddress.AddressLine1)}", $"The length of 'Person Address Address Line1' must be {AttributeConstraints.Contact.Address1_Line1MaxLength} characters or fewer. You entered 255 characters."},
                { $"{nameof(CreateTrnRequestRequest.Person)}.{nameof(CreateTrnRequestRequestPerson.Address)}.{nameof(CreateTrnRequestAddress.AddressLine2)}", $"The length of 'Person Address Address Line2' must be {AttributeConstraints.Contact.Address1_Line2MaxLength} characters or fewer. You entered 255 characters."},
                { $"{nameof(CreateTrnRequestRequest.Person)}.{nameof(CreateTrnRequestRequestPerson.Address)}.{nameof(CreateTrnRequestAddress.AddressLine3)}", $"The length of 'Person Address Address Line3' must be {AttributeConstraints.Contact.Address1_Line3MaxLength} characters or fewer. You entered 255 characters."},
                { $"{nameof(CreateTrnRequestRequest.Person)}.{nameof(CreateTrnRequestRequestPerson.Address)}.{nameof(CreateTrnRequestAddress.Postcode)}", $"The length of 'Person Address Postcode' must be {AttributeConstraints.Contact.Address1_PostalCodeLength} characters or fewer. You entered 255 characters."},
                { $"{nameof(CreateTrnRequestRequest.Person)}.{nameof(CreateTrnRequestRequestPerson.Address)}.{nameof(CreateTrnRequestAddress.City)}", $"The length of 'Person Address City' must be {AttributeConstraints.Contact.Address1_CityMaxLength} characters or fewer. You entered 255 characters."},
                { $"{nameof(CreateTrnRequestRequest.Person)}.{nameof(CreateTrnRequestRequestPerson.Address)}.{nameof(CreateTrnRequestAddress.Country)}", $"The length of 'Person Address Country' must be {AttributeConstraints.Contact.Address1_CountryMaxLength} characters or fewer. You entered 255 characters."},
            });
    }
}
