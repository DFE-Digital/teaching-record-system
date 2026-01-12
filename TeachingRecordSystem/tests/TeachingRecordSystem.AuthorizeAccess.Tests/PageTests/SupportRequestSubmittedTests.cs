// using System.Diagnostics;
//
// namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests;
//
// public class SupportRequestSubmittedTests(HostFixture hostFixture) : TestBase(hostFixture)
// {
//     [Fact]
//     public async Task Get_NotAuthenticatedWithOneLogin_ReturnsBadRequest()
//     {
//         // Arrange
//         var state = CreateNewState();
//         var journeyInstance = await CreateJourneyInstance(state);
//
//         var request = new HttpRequestMessage(HttpMethod.Get, $"/request-submitted?{journeyInstance.GetUniqueIdQueryParameter()}");
//
//         // Act
//         var response = await HttpClient.SendAsync(request);
//
//         // Assert
//         Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
//     }
//
//     [Fact]
//     public async Task Get_NotVerifiedWithOneLogin_ReturnsBadRequest()
//     {
//         // Arrange
//         var state = CreateNewState();
//         var journeyInstance = await CreateJourneyInstance(state);
//
//         var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, createCoreIdentityVc: false);
//         await WithSignInJourneyCoordinator(helper => helper.OnUserAuthenticatedAsync(journeyInstance, ticket));
//
//         var request = new HttpRequestMessage(HttpMethod.Get, $"/request-submitted?{journeyInstance.GetUniqueIdQueryParameter()}");
//
//         // Act
//         var response = await HttpClient.SendAsync(request);
//
//         // Assert
//         Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
//     }
//
//     [Fact]
//     public async Task Get_NationalInsuranceNumberNotSpecified_RedirectsToNationalInsuranceNumberPage()
//     {
//         // Arrange
//         var state = CreateNewState();
//         var journeyInstance = await CreateJourneyInstance(state);
//
//         var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);
//
//         var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, oneLoginUser);
//         await WithSignInJourneyCoordinator(helper => helper.OnUserAuthenticatedAsync(journeyInstance, ticket));
//
//         Debug.Assert(state.NationalInsuranceNumber is null);
//
//         var request = new HttpRequestMessage(HttpMethod.Get, $"/request-submitted?{journeyInstance.GetUniqueIdQueryParameter()}");
//
//         // Act
//         var response = await HttpClient.SendAsync(request);
//
//         // Assert
//         Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
//         Assert.Equal($"/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
//     }
//
//     [Fact]
//     public async Task Get_TrnNotSpecified_RedirectsToTrnPage()
//     {
//         // Arrange
//         var state = CreateNewState();
//         var journeyInstance = await CreateJourneyInstance(state);
//
//         var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);
//
//         var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, oneLoginUser);
//         await WithSignInJourneyCoordinator(helper => helper.OnUserAuthenticatedAsync(journeyInstance, ticket));
//
//         Debug.Assert(state.NationalInsuranceNumber is null);
//         await journeyInstance.UpdateStateAsync(state => state.SetNationalInsuranceNumber(true, TestData.GenerateNationalInsuranceNumber()));
//
//         var request = new HttpRequestMessage(HttpMethod.Get, $"/request-submitted?{journeyInstance.GetUniqueIdQueryParameter()}");
//
//         // Act
//         var response = await HttpClient.SendAsync(request);
//
//         // Assert
//         Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
//         Assert.Equal($"/trn?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
//     }
//
//     [Fact]
//     public async Task Get_AlreadyAuthenticated_RedirectsToStateRedirectUri()
//     {
//         // Arrange
//         var state = CreateNewState();
//         var journeyInstance = await CreateJourneyInstance(state);
//
//         var person = await TestData.CreatePersonAsync();
//         var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);
//
//         var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, oneLoginUser);
//         await WithSignInJourneyCoordinator(helper => helper.OnUserAuthenticatedAsync(journeyInstance, ticket));
//
//         var request = new HttpRequestMessage(HttpMethod.Get, $"/request-submitted?{journeyInstance.GetUniqueIdQueryParameter()}");
//
//         // Act
//         var response = await HttpClient.SendAsync(request);
//
//         // Assert
//         Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
//         Assert.Equal($"{state.RedirectUri}?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
//     }
//
//     [Fact]
//     public async Task Get_SupportTicketNotCreated_RedirectsToCheckAnswersPage()
//     {
//         // Arrange
//         var state = CreateNewState();
//         var journeyInstance = await CreateJourneyInstance(state);
//
//         var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);
//
//         var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, oneLoginUser);
//         await WithSignInJourneyCoordinator(helper => helper.OnUserAuthenticatedAsync(journeyInstance, ticket));
//
//         var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
//         var trn = await TestData.GenerateTrnAsync();
//
//         await journeyInstance.UpdateStateAsync(state =>
//         {
//             state.SetNationalInsuranceNumber(true, nationalInsuranceNumber);
//             state.SetTrn(true, trn);
//         });
//
//         var request = new HttpRequestMessage(HttpMethod.Get, $"/request-submitted?{journeyInstance.GetUniqueIdQueryParameter()}");
//
//         // Act
//         var response = await HttpClient.SendAsync(request);
//
//         // Assert
//         Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
//         Assert.Equal($"/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
//     }
//
//     [Fact]
//     public async Task Get_ValidRequest_ReturnsExpectedContent()
//     {
//         // Arrange
//         var state = CreateNewState();
//         var journeyInstance = await CreateJourneyInstance(state);
//
//         var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);
//
//         var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, oneLoginUser);
//         await WithSignInJourneyCoordinator(helper => helper.OnUserAuthenticatedAsync(journeyInstance, ticket));
//
//         var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
//         var trn = await TestData.GenerateTrnAsync();
//
//         await journeyInstance.UpdateStateAsync(state =>
//         {
//             state.SetNationalInsuranceNumber(true, nationalInsuranceNumber);
//             state.SetTrn(true, trn);
//             state.HasPendingSupportRequest = true;
//         });
//
//         var request = new HttpRequestMessage(HttpMethod.Get, $"/request-submitted?{journeyInstance.GetUniqueIdQueryParameter()}");
//
//         // Act
//         var response = await HttpClient.SendAsync(request);
//
//         // Assert
//         await AssertEx.HtmlResponseAsync(response);
//     }
// }
