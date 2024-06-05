namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests.RequestTrn;

public class AddressTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_HasPendingTrnRequestSetTrue_RedirectsToSubmitted()
    {
        // Arrange
        var state = CreateNewState();
        state.HasPendingTrnRequest = true;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/address?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/submitted?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_HasNationalInsuranceNumberMissingFromState_RedirectsToNationalInsuranceNumber()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/address?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ValidRequestWithPopulatedDataInJourneyState_PopulatesModelFromJourneyState()
    {
        // Arrange
        var state = CreateNewState();
        state.Email = Faker.Internet.Email();
        state.Name = Faker.Name.FullName();
        state.PreviousName = Faker.Name.FullName();
        state.HasPreviousName = true;
        state.DateOfBirth = new DateOnly(1980, 12, 13);
        state.HasNationalInsuranceNumber = false;
        state.AddressLine1 = Faker.Address.StreetAddress();
        state.AddressLine2 = Faker.Address.SecondaryAddress();
        state.TownOrCity = Faker.Address.City();
        state.Country = Faker.Address.Country();
        state.PostalCode = Faker.Address.ZipCode();
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/address?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        Assert.Equal(state.AddressLine1, doc.GetElementById("AddressLine1")?.GetAttribute("value"));
        Assert.Equal(state.AddressLine2, doc.GetElementById("AddressLine2")?.GetAttribute("value"));
        Assert.Equal(state.TownOrCity, doc.GetElementById("TownOrCity")?.GetAttribute("value"));
        Assert.Equal(state.PostalCode, doc.GetElementById("PostalCode")?.GetAttribute("value"));
        Assert.Equal(state.Country, doc.GetElementById("Country")?.GetAttribute("value"));
    }

    [Fact]
    public async Task Post_HasPendingTrnRequestSetTrue_RedirectsToSubmitted()
    {
        // Arrange
        var state = CreateNewState();
        state.HasPendingTrnRequest = true;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/address?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/submitted?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_HasNationalInsuranceNumberMissingFromState_RedirectsToNationalInsuranceNumber()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/address?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(AddressLineType.AddressLine1, "Enter address line 1")]
    [InlineData(AddressLineType.TownOrCity, "Enter town or city")]
    [InlineData(AddressLineType.PostalCode, "Enter postal code")]
    [InlineData(AddressLineType.Country, "Enter country")]
    public async Task Post_EmptyMandatoryAddressLine_ReturnsError(AddressLineType emptyAddressLineType, string errorMessage)
    {
        // Arrange
        var state = CreateNewState();
        state.Email = Faker.Internet.Email();
        state.Name = Faker.Name.FullName();
        state.PreviousName = Faker.Name.FullName();
        state.HasPreviousName = true;
        state.DateOfBirth = new DateOnly(1980, 12, 13);
        state.HasNationalInsuranceNumber = false;
        var journeyInstance = await CreateJourneyInstance(state);

        var content = new FormUrlEncodedContentBuilder();
        if (emptyAddressLineType != AddressLineType.AddressLine1)
        {
            content.Add("AddressLine1", Faker.Address.StreetAddress());
        }

        if (emptyAddressLineType != AddressLineType.AddressLine2)
        {
            content.Add("AddressLine2", Faker.Address.SecondaryAddress());
        }

        if (emptyAddressLineType != AddressLineType.TownOrCity)
        {
            content.Add("TownOrCity", Faker.Address.City());
        }

        if (emptyAddressLineType != AddressLineType.PostalCode)
        {
            content.Add("PostalCode", Faker.Address.ZipCode());
        }

        if (emptyAddressLineType != AddressLineType.Country)
        {
            content.Add("Country", Faker.Address.Country());
        }

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/address?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = content
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, emptyAddressLineType.ToString(), errorMessage);
    }

    [Theory]
    [InlineData(AddressLineType.AddressLine1, "Address line 1 must be 200 characters or less")]
    [InlineData(AddressLineType.AddressLine2, "Address line 2 must be 200 characters or less")]
    [InlineData(AddressLineType.TownOrCity, "Town or city must be 200 characters or less")]
    [InlineData(AddressLineType.PostalCode, "Postal code must be 50 characters or less")]
    [InlineData(AddressLineType.Country, "Country must be 200 characters or less")]
    public async Task Post_AddressLinesTooManyCharacters_ReturnsError(AddressLineType overflowAddressLineType, string errorMessage)
    {
        // Arrange
        var state = CreateNewState();
        state.Email = Faker.Internet.Email();
        state.Name = Faker.Name.FullName();
        state.PreviousName = Faker.Name.FullName();
        state.HasPreviousName = true;
        state.DateOfBirth = new DateOnly(1980, 12, 13);
        state.HasNationalInsuranceNumber = false;
        var journeyInstance = await CreateJourneyInstance(state);

        var content = new FormUrlEncodedContentBuilder();
        var addressLine1 = overflowAddressLineType == AddressLineType.AddressLine1 ? new string('a', 201) : Faker.Address.StreetAddress();
        var addressLine2 = overflowAddressLineType == AddressLineType.AddressLine2 ? new string('a', 201) : Faker.Address.SecondaryAddress();
        var townOrCity = overflowAddressLineType == AddressLineType.TownOrCity ? new string('a', 201) : Faker.Address.City();
        var postalCode = overflowAddressLineType == AddressLineType.PostalCode ? new string('a', 51) : Faker.Address.ZipCode();
        var country = overflowAddressLineType == AddressLineType.Country ? new string('a', 201) : Faker.Address.Country();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/address?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "AddressLine1", addressLine1 },
                { "AddressLine2", addressLine2 },
                { "TownOrCity", townOrCity },
                { "PostalCode", postalCode },
                { "Country", country }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, overflowAddressLineType.ToString(), errorMessage);
    }

    [Fact]
    public async Task Post_ValidRequest_UpdatesStateAndRedirectsToNextPage()
    {
        // Arrange
        var state = CreateNewState();
        state.Email = Faker.Internet.Email();
        state.Name = Faker.Name.FullName();
        state.PreviousName = Faker.Name.FullName();
        state.HasPreviousName = true;
        state.DateOfBirth = new DateOnly(1980, 12, 13);
        state.HasNationalInsuranceNumber = false;
        var journeyInstance = await CreateJourneyInstance(state);

        var addressLine1 = Faker.Address.StreetAddress();
        var addressLine2 = Faker.Address.SecondaryAddress();
        var townOrCity = Faker.Address.City();
        var postalCode = Faker.Address.ZipCode();
        var country = Faker.Address.Country();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/address?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "AddressLine1", addressLine1 },
                { "AddressLine2", addressLine2 },
                { "TownOrCity", townOrCity },
                { "PostalCode", postalCode },
                { "Country", country }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        var reloadedJourneyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(addressLine1, reloadedJourneyInstance.State.AddressLine1);
        Assert.Equal(addressLine2, reloadedJourneyInstance.State.AddressLine2);
        Assert.Equal(townOrCity, reloadedJourneyInstance.State.TownOrCity);
        Assert.Equal(postalCode, reloadedJourneyInstance.State.PostalCode);
        Assert.Equal(country, reloadedJourneyInstance.State.Country);
    }

    public enum AddressLineType
    {
        AddressLine1,
        AddressLine2,
        TownOrCity,
        PostalCode,
        Country
    }
}
