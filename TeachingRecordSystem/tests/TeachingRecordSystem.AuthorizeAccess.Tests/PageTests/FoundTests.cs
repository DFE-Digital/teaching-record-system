// using System.Diagnostics;
//
// namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests;
//
// public class FoundTests(HostFixture hostFixture) : TestBase(hostFixture)
// {
//     [Fact]
//     public async Task Get_NotFullyAuthenticated_RedirectsToStartOfMatchingJourney()
//     {
//         // Arrange
//         var state = CreateNewState();
//         var journeyInstance = await CreateJourneyInstance(state);
//
//         var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);
//
//         var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, oneLoginUser);
//         await WithSignInJourneyCoordinator(helper => helper.OnOneLoginCallbackAsync(journeyInstance, ticket));
//         Debug.Assert(state.AuthenticationTicket is null);
//
//         var request = new HttpRequestMessage(HttpMethod.Get, $"/found?{journeyInstance.GetUniqueIdQueryParameter()}");
//
//         // Act
//         var response = await HttpClient.SendAsync(request);
//
//         // Assert
//         Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
//         Assert.Equal($"/connect?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
//     }
//
//     [Fact]
//     public async Task Get_ValidRequest_RendersExpectedContent()
//     {
//         // Arrange
//         var state = CreateNewState();
//         var journeyInstance = await CreateJourneyInstance(state);
//
//         var person = await TestData.CreatePersonAsync();
//         var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);
//
//         var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, oneLoginUser);
//         await WithSignInJourneyCoordinator(helper => helper.OnOneLoginCallbackAsync(journeyInstance, ticket));
//
//         var request = new HttpRequestMessage(HttpMethod.Get, $"/found?{journeyInstance.GetUniqueIdQueryParameter()}");
//
//         // Act
//         var response = await HttpClient.SendAsync(request);
//
//         // Assert
//         await AssertEx.HtmlResponseAsync(response);
//     }
//
//     [Fact]
//     public async Task Post_NotAuthenticated_RedirectsToStartOfMatchingJourney()
//     {
//         // Arrange
//         var state = CreateNewState();
//         var journeyInstance = await CreateJourneyInstance(state);
//
//         var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);
//
//         var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, oneLoginUser);
//         await WithSignInJourneyCoordinator(helper => helper.OnOneLoginCallbackAsync(journeyInstance, ticket));
//         Debug.Assert(state.AuthenticationTicket is null);
//
//         var request = new HttpRequestMessage(HttpMethod.Post, $"/found?{journeyInstance.GetUniqueIdQueryParameter()}");
//
//         // Act
//         var response = await HttpClient.SendAsync(request);
//
//         // Assert
//         Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
//         Assert.Equal($"/connect?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
//     }
//
//     [Fact]
//     public async Task Post_ValidRequest_RedirectsToStateRedirectUri()
//     {
//         // Arrange
//         var state = CreateNewState();
//         var journeyInstance = await CreateJourneyInstance(state);
//
//         var person = await TestData.CreatePersonAsync();
//         var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);
//
//         var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, oneLoginUser);
//         await WithSignInJourneyCoordinator(helper => helper.OnOneLoginCallbackAsync(journeyInstance, ticket));
//
//         var request = new HttpRequestMessage(HttpMethod.Post, $"/found?{journeyInstance.GetUniqueIdQueryParameter()}");
//
//         // Act
//         var response = await HttpClient.SendAsync(request);
//
//         // Assert
//         Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
//         Assert.Equal($"{state.RedirectUri}?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
//     }
// }
