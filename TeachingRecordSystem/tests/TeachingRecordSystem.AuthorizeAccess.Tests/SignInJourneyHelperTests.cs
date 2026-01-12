// using System.Diagnostics;
// using System.Security.Claims;
// using System.Security.Cryptography;
// using GovUk.OneLogin.AspNetCore;
// using Microsoft.AspNetCore.Http.HttpResults;
// using Microsoft.Extensions.Options;
// using Optional;
// using TeachingRecordSystem.Core.DataStore.Postgres;
// using TeachingRecordSystem.Core.Jobs.Scheduling;
// using TeachingRecordSystem.Core.Services.Notify;
// using TeachingRecordSystem.Core.Services.OneLogin;
// using static TeachingRecordSystem.AuthorizeAccess.IdModelTypes;
//
// namespace TeachingRecordSystem.AuthorizeAccess.Tests;
//
// public class SignInJourneyHelperTests(HostFixture hostFixture) : TestBase(hostFixture)
// {
//     [Fact]
//     public Task OnOneLoginCallback_AuthenticationOnly_UserAlreadyExistsAndTeachingRecordKnown_CompletesJourney() =>
//         WithDbContextAsync(async dbContext =>
//         {
//             // Arrange
//             var helper = CreateHelper(dbContext);
//
//             var person = await TestData.CreatePersonAsync();
//             var user = await TestData.CreateOneLoginUserAsync(person);
//             Clock.Advance();
//
//             var state = new SignInJourneyState(
//                 redirectUri: "/",
//                 serviceName: "Test Service",
//                 serviceUrl: "https://service",
//                 oneLoginAuthenticationScheme: "dummy",
//                 clientApplicationUserId: default);
//             var journeyInstance = await CreateJourneyInstance(state);
//
//             var authenticationTicket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, sub: user.Subject);
//
//             // Act
//             var result = await helper.OnOneLoginCallbackAsync(journeyInstance, authenticationTicket);
//
//             // Assert
//             Assert.NotNull(state.AuthenticationTicket);
//
//             user = await WithDbContextAsync(dbContext => dbContext.OneLoginUsers.SingleAsync(u => u.Subject == user.Subject));
//             Assert.NotEqual(Clock.UtcNow, user.FirstOneLoginSignIn);
//             Assert.Equal(Clock.UtcNow, user.LastOneLoginSignIn);
//             Assert.NotEqual(Clock.UtcNow, user.FirstSignIn);
//             Assert.Equal(Clock.UtcNow, user.LastSignIn);
//
//             var redirectResult = Assert.IsType<RedirectHttpResult>(result);
//             Assert.Equal($"{state.RedirectUri}?{journeyInstance.GetUniqueIdQueryParameter()}", redirectResult.Url);
//         });
//
//     [Fact]
//     public Task OnOneLoginCallback_AuthenticationOnly_UserAlreadyExistsWithSubjectOnlyAndTeachingRecordKnown_AssignsEmailAndSignInFieldsAndCompletesJourney() =>
//         WithDbContextAsync(async dbContext =>
//         {
//             // Arrange
//             var helper = CreateHelper(dbContext);
//
//             var person = await TestData.CreatePersonAsync();
//             var user = await TestData.CreateOneLoginUserAsync(person, email: Option.Some((string?)null));
//             Clock.Advance();
//
//             var state = new SignInJourneyState(
//                 redirectUri: "/",
//                 serviceName: "Test Service",
//                 serviceUrl: "https://service",
//                 oneLoginAuthenticationScheme: "dummy",
//                 clientApplicationUserId: default);
//             var journeyInstance = await CreateJourneyInstance(state);
//
//             var email = Faker.Internet.Email();
//             var authenticationTicket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, sub: user.Subject, email);
//
//             // Act
//             var result = await helper.OnOneLoginCallbackAsync(journeyInstance, authenticationTicket);
//
//             // Assert
//             Assert.NotNull(state.AuthenticationTicket);
//
//             user = await WithDbContextAsync(dbContext => dbContext.OneLoginUsers.SingleAsync(u => u.Subject == user.Subject));
//             Assert.Equal(email, user.EmailAddress);
//             Assert.Equal(Clock.UtcNow, user.FirstOneLoginSignIn);
//             Assert.Equal(Clock.UtcNow, user.LastOneLoginSignIn);
//             Assert.Equal(Clock.UtcNow, user.FirstSignIn);
//             Assert.Equal(Clock.UtcNow, user.LastSignIn);
//
//             var redirectResult = Assert.IsType<RedirectHttpResult>(result);
//             Assert.Equal($"{state.RedirectUri}?{journeyInstance.GetUniqueIdQueryParameter()}", redirectResult.Url);
//         });
//
//     [Fact]
//     public Task OnOneLoginCallback_AuthenticationOnly_NewUserWithResolvedTrnRequestMatchedOnOneLoginUserSubject_SetsUserVerifiedAssignsTrnAndCompletesJourney() =>
//         WithDbContextAsync(async dbContext =>
//         {
//             // Arrange
//             var helper = CreateHelper(dbContext);
//
//             var state = new SignInJourneyState(
//                 redirectUri: "/",
//                 serviceName: "Test Service",
//                 serviceUrl: "https://service",
//                 oneLoginAuthenticationScheme: "dummy",
//                 clientApplicationUserId: default);
//             var journeyInstance = await CreateJourneyInstance(state);
//
//             var subject = TestData.CreateOneLoginUserSubject();
//
//             var trnRequestId = Guid.NewGuid().ToString();
//             var trnRequestFromApplicationUser = await TestData.CreateApplicationUserAsync();
//             var person = await TestData.CreatePersonAsync(p => p
//                 .WithTrnRequest(trnRequestFromApplicationUser.UserId, trnRequestId, identityVerified: true, oneLoginUserSubject: subject));
//
//             var authenticationTicket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, sub: subject);
//
//             // Act
//             var result = await helper.OnOneLoginCallbackAsync(journeyInstance, authenticationTicket);
//
//             // Assert
//             Assert.NotNull(state.AuthenticationTicket);
//
//             var user = await WithDbContextAsync(dbContext => dbContext.OneLoginUsers.SingleAsync(u => u.Subject == subject));
//             Assert.Equal(Clock.UtcNow, user.FirstOneLoginSignIn);
//             Assert.Equal(Clock.UtcNow, user.LastOneLoginSignIn);
//             Assert.Equal(Clock.UtcNow, user.FirstSignIn);
//             Assert.Equal(Clock.UtcNow, user.LastSignIn);
//
//             var redirectResult = Assert.IsType<RedirectHttpResult>(result);
//             Assert.Equal($"{state.RedirectUri}?{journeyInstance.GetUniqueIdQueryParameter()}", redirectResult.Url);
//         });
//
//     [Fact]
//     public Task OnOneLoginCallback_AuthenticationOnly_NewUserWithPendingTrnRequestMatchedOnOneLoginUserSubject_RequestsIdentityVerification() =>
//         WithDbContextAsync(async dbContext =>
//         {
//             // Arrange
//             var helper = CreateHelper(dbContext);
//
//             var state = new SignInJourneyState(
//                 redirectUri: "/",
//                 serviceName: "Test Service",
//                 serviceUrl: "https://service",
//                 oneLoginAuthenticationScheme: "dummy",
//                 clientApplicationUserId: default);
//             var journeyInstance = await CreateJourneyInstance(state);
//
//             var subject = TestData.CreateOneLoginUserSubject();
//
//             var trnRequestId = Guid.NewGuid().ToString();
//             var trnRequestFromApplicationUser = await TestData.CreateApplicationUserAsync();
//             await TestData.CreateApiTrnRequestSupportTaskAsync(
//                 trnRequestFromApplicationUser.UserId,
//                 s => s.WithStatus(SupportTaskStatus.Open).WithIdentityVerified(true).WithOneLoginUserSubject(subject));
//
//             var authenticationTicket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, sub: subject);
//
//             // Act
//             var result = await helper.OnOneLoginCallbackAsync(journeyInstance, authenticationTicket);
//
//             // Assert
//             Assert.NotNull(state.OneLoginAuthenticationTicket);
//             Assert.Null(state.AuthenticationTicket);
//
//             var challengeResult = Assert.IsType<ChallengeHttpResult>(result);
//             Assert.Collection(
//                 challengeResult.Properties!.GetVectorsOfTrust(),
//                 vtr => Assert.Equal(SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr, vtr));
//         });
//
//     [Fact]
//     public Task OnOneLoginCallback_AuthenticationOnly_NewUserWithResolvedTrnRequestMatchedOnEmail_SetsUserVerifiedAssignsTrnAndCompletesJourney() =>
//         WithDbContextAsync(async dbContext =>
//         {
//             // Arrange
//             var helper = CreateHelper(dbContext);
//
//             var state = new SignInJourneyState(
//                 redirectUri: "/",
//                 serviceName: "Test Service",
//                 serviceUrl: "https://service",
//                 oneLoginAuthenticationScheme: "dummy",
//                 clientApplicationUserId: default);
//             var journeyInstance = await CreateJourneyInstance(state);
//
//             var subject = TestData.CreateOneLoginUserSubject();
//             var email = TestData.GenerateUniqueEmail();
//
//             var trnRequestId = Guid.NewGuid().ToString();
//             var trnRequestFromApplicationUser = await TestData.CreateApplicationUserAsync();
//             var person = await TestData.CreatePersonAsync(p => p
//                 .WithEmailAddress(email)
//                 .WithTrnRequest(trnRequestFromApplicationUser.UserId, trnRequestId, identityVerified: true));
//
//             var authenticationTicket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, sub: subject, email: email);
//
//             // Act
//             var result = await helper.OnOneLoginCallbackAsync(journeyInstance, authenticationTicket);
//
//             // Assert
//             Assert.NotNull(state.AuthenticationTicket);
//
//             var user = await WithDbContextAsync(dbContext => dbContext.OneLoginUsers.SingleAsync(u => u.Subject == subject));
//             Assert.Equal(Clock.UtcNow, user.FirstOneLoginSignIn);
//             Assert.Equal(Clock.UtcNow, user.LastOneLoginSignIn);
//             Assert.Equal(Clock.UtcNow, user.FirstSignIn);
//             Assert.Equal(Clock.UtcNow, user.LastSignIn);
//
//             var redirectResult = Assert.IsType<RedirectHttpResult>(result);
//             Assert.Equal($"{state.RedirectUri}?{journeyInstance.GetUniqueIdQueryParameter()}", redirectResult.Url);
//         });
//
//     [Fact]
//     public Task OnOneLoginCallback_AuthenticationOnly_NewUserWithPendingTrnRequestMatchedOnEmail_RequestsIdentityVerification() =>
//         WithDbContextAsync(async dbContext =>
//         {
//             // Arrange
//             var helper = CreateHelper(dbContext);
//
//             var state = new SignInJourneyState(
//                 redirectUri: "/",
//                 serviceName: "Test Service",
//                 serviceUrl: "https://service",
//                 oneLoginAuthenticationScheme: "dummy",
//                 clientApplicationUserId: default);
//             var journeyInstance = await CreateJourneyInstance(state);
//
//             var subject = TestData.CreateOneLoginUserSubject();
//             var email = TestData.GenerateUniqueEmail();
//
//             var trnRequestId = Guid.NewGuid().ToString();
//             var trnRequestFromApplicationUser = await TestData.CreateApplicationUserAsync();
//             var person = await TestData.CreatePersonAsync(p => p
//                 .WithEmailAddress(email)
//                 .WithTrnRequest(trnRequestFromApplicationUser.UserId, trnRequestId, identityVerified: true));
//
//             var authenticationTicket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, sub: subject);
//
//             // Act
//             var result = await helper.OnOneLoginCallbackAsync(journeyInstance, authenticationTicket);
//
//             // Assert
//             Assert.NotNull(state.OneLoginAuthenticationTicket);
//             Assert.Null(state.AuthenticationTicket);
//
//             var challengeResult = Assert.IsType<ChallengeHttpResult>(result);
//             Assert.Collection(
//                 challengeResult.Properties!.GetVectorsOfTrust(),
//                 vtr => Assert.Equal(SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr, vtr));
//         });
//
//     [Fact]
//     public Task OnOneLoginCallback_AuthenticationOnly_NewUserWithTrnRequestMatchedOnOneLoginUserSubjectWithoutIdentityVerified_RequestsIdentityVerification() =>
//         WithDbContextAsync(async dbContext =>
//         {
//             // Arrange
//             var helper = CreateHelper(dbContext);
//
//             var state = new SignInJourneyState(
//                 redirectUri: "/",
//                 serviceName: "Test Service",
//                 serviceUrl: "https://service",
//                 oneLoginAuthenticationScheme: "dummy",
//                 clientApplicationUserId: default);
//             var journeyInstance = await CreateJourneyInstance(state);
//
//             var subject = TestData.CreateOneLoginUserSubject();
//
//             var trnRequestId = Guid.NewGuid().ToString();
//             var trnRequestFromApplicationUser = await TestData.CreateApplicationUserAsync();
//             var person = await TestData.CreatePersonAsync(p => p
//                 .WithTrnRequest(trnRequestFromApplicationUser.UserId, trnRequestId, identityVerified: false, oneLoginUserSubject: subject));
//
//             var authenticationTicket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, sub: subject);
//
//             // Act
//             var result = await helper.OnOneLoginCallbackAsync(journeyInstance, authenticationTicket);
//
//             // Assert
//             Assert.NotNull(state.OneLoginAuthenticationTicket);
//             Assert.Null(state.AuthenticationTicket);
//
//             var challengeResult = Assert.IsType<ChallengeHttpResult>(result);
//             Assert.Collection(
//                 challengeResult.Properties!.GetVectorsOfTrust(),
//                 vtr => Assert.Equal(SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr, vtr));
//         });
//
//     [Fact]
//     public Task OnOneLoginCallback_AuthenticationOnly_NewUserWithTrnRequestMatchedOnEmailWithoutIdentityVerified_RequestsIdentityVerification() =>
//         WithDbContextAsync(async dbContext =>
//         {
//             // Arrange
//             var helper = CreateHelper(dbContext);
//
//             var state = new SignInJourneyState(
//                 redirectUri: "/",
//                 serviceName: "Test Service",
//                 serviceUrl: "https://service",
//                 oneLoginAuthenticationScheme: "dummy",
//                 clientApplicationUserId: default);
//             var journeyInstance = await CreateJourneyInstance(state);
//
//             var subject = TestData.CreateOneLoginUserSubject();
//             var email = TestData.GenerateUniqueEmail();
//
//             var trnRequestId = Guid.NewGuid().ToString();
//             var trnRequestFromApplicationUser = await TestData.CreateApplicationUserAsync();
//             var person = await TestData.CreatePersonAsync(p => p
//                 .WithEmailAddress(email)
//                 .WithTrnRequest(trnRequestFromApplicationUser.UserId, trnRequestId, identityVerified: false));
//
//             var authenticationTicket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, sub: subject);
//
//             // Act
//             var result = await helper.OnOneLoginCallbackAsync(journeyInstance, authenticationTicket);
//
//             // Assert
//             Assert.NotNull(state.OneLoginAuthenticationTicket);
//             Assert.Null(state.AuthenticationTicket);
//
//             var challengeResult = Assert.IsType<ChallengeHttpResult>(result);
//             Assert.Collection(
//                 challengeResult.Properties!.GetVectorsOfTrust(),
//                 vtr => Assert.Equal(SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr, vtr));
//         });
//
//     [Fact]
//     public Task OnOneLoginCallback_AuthenticationOnly_UserAlreadyExistsButTeachingNotRecordKnown_RequestsIdentityVerification() =>
//         WithDbContextAsync(async dbContext =>
//         {
//             // Arrange
//             var helper = CreateHelper(dbContext);
//
//             var user = await TestData.CreateOneLoginUserAsync(personId: null);
//             Clock.Advance();
//
//             var state = new SignInJourneyState(
//                 redirectUri: "/",
//                 serviceName: "Test Service",
//                 serviceUrl: "https://service",
//                 oneLoginAuthenticationScheme: "dummy",
//                 clientApplicationUserId: default);
//             var journeyInstance = await CreateJourneyInstance(state);
//
//             var authenticationTicket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, sub: user.Subject);
//
//             // Act
//             var result = await helper.OnOneLoginCallbackAsync(journeyInstance, authenticationTicket);
//
//             // Assert
//             Assert.NotNull(state.OneLoginAuthenticationTicket);
//             Assert.Null(state.AuthenticationTicket);
//
//             user = await WithDbContextAsync(dbContext => dbContext.OneLoginUsers.SingleAsync(u => u.Subject == user.Subject));
//             Assert.NotEqual(Clock.UtcNow, user.FirstOneLoginSignIn);
//             Assert.Equal(Clock.UtcNow, user.LastOneLoginSignIn);
//             Assert.NotEqual(Clock.UtcNow, user.FirstSignIn);
//             Assert.NotEqual(Clock.UtcNow, user.LastSignIn);
//
//             var challengeResult = Assert.IsType<ChallengeHttpResult>(result);
//             Assert.Collection(
//                 challengeResult.Properties!.GetVectorsOfTrust(),
//                 vtr => Assert.Equal(SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr, vtr));
//         });
//
//     [Fact]
//     public Task OnOneLoginCallback_AuthenticationOnly_UserDoesNotExist_RequestsIdentityVerification() =>
//         WithDbContextAsync(async dbContext =>
//         {
//             // Arrange
//             var helper = CreateHelper(dbContext);
//
//             var state = new SignInJourneyState(
//                 redirectUri: "/",
//                 serviceName: "Test Service",
//                 serviceUrl: "https://service",
//                 oneLoginAuthenticationScheme: "dummy",
//                 clientApplicationUserId: default);
//             var journeyInstance = await CreateJourneyInstance(state);
//
//             var subject = TestData.CreateOneLoginUserSubject();
//             var authenticationTicket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, sub: subject);
//
//             // Act
//             var result = await helper.OnOneLoginCallbackAsync(journeyInstance, authenticationTicket);
//
//             // Assert
//             Assert.NotNull(state.OneLoginAuthenticationTicket);
//             Assert.Null(state.AuthenticationTicket);
//
//             var user = await WithDbContextAsync(dbContext => dbContext.OneLoginUsers.SingleOrDefaultAsync(u => u.Subject == subject));
//             Assert.NotNull(user);
//             Assert.Equal(Clock.UtcNow, user.FirstOneLoginSignIn);
//             Assert.Null(user.FirstSignIn);
//             Assert.Equal(Clock.UtcNow, user.LastOneLoginSignIn);
//             Assert.NotEqual(Clock.UtcNow, user.LastSignIn);
//
//             var challengeResult = Assert.IsType<ChallengeHttpResult>(result);
//             Assert.Collection(
//                 challengeResult.Properties!.GetVectorsOfTrust(),
//                 vtr => Assert.Equal(SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr, vtr));
//         });
//
//     [Fact]
//     public Task OnOneLoginCallback_AuthenticationAndVerification_VerificationFailed_RedirectsToErrorPage() =>
//         WithDbContextAsync(async dbContext =>
//         {
//             // Arrange
//             var helper = CreateHelper(dbContext);
//
//             var user = await TestData.CreateOneLoginUserAsync(personId: null);
//             Clock.Advance();
//
//             var state = new SignInJourneyState(
//                 redirectUri: "/",
//                 serviceName: "Test Service",
//                 serviceUrl: "https://service",
//                 oneLoginAuthenticationScheme: "dummy",
//                 clientApplicationUserId: default);
//             var journeyInstance = await CreateJourneyInstance(state);
//
//             var authenticationTicket = CreateOneLoginAuthenticationTicket(
//                 vtr: SignInJourneyHelper.AuthenticationOnlyVtr,
//                 sub: user.Subject,
//                 email: user.EmailAddress,
//                 createCoreIdentityVc: false);
//             var callbackResult = await helper.OnOneLoginCallbackAsync(journeyInstance, authenticationTicket);
//             Debug.Assert(callbackResult is ChallengeHttpResult);
//
//             // Act
//             var result = await helper.OnVerificationFailedAsync(journeyInstance);
//
//             // Assert
//             Assert.NotNull(state.OneLoginAuthenticationTicket);
//             Assert.Null(state.AuthenticationTicket);
//
//             user = await WithDbContextAsync(dbContext => dbContext.OneLoginUsers.SingleAsync(u => u.Subject == user.Subject));
//             Assert.NotEqual(Clock.UtcNow, user.FirstOneLoginSignIn);
//             Assert.Null(user.FirstSignIn);
//             Assert.Equal(Clock.UtcNow, user.LastOneLoginSignIn);
//             Assert.NotEqual(Clock.UtcNow, user.LastSignIn);
//
//             var redirectResult = Assert.IsType<RedirectHttpResult>(result);
//             Assert.Equal($"/NotVerified?{journeyInstance.GetUniqueIdQueryParameter()}", redirectResult.Url);
//         });
//
//     [Fact]
//     public Task OnOneLoginCallback_AuthenticationAndVerification_VerificationSucceeded_RedirectsToStartOfMatchingJourney() =>
//         WithDbContextAsync(async dbContext =>
//         {
//             // Arrange
//             var helper = CreateHelper(dbContext);
//
//             var user = await TestData.CreateOneLoginUserAsync(personId: null);
//             Clock.Advance();
//
//             var firstName = Faker.Name.First();
//             var lastName = Faker.Name.Last();
//             var dateOfBirth = DateOnly.FromDateTime(Faker.Identification.DateOfBirth());
//
//             var state = new SignInJourneyState(
//                 redirectUri: "/",
//                 serviceName: "Test Service",
//                 serviceUrl: "https://service",
//                 oneLoginAuthenticationScheme: "dummy",
//                 clientApplicationUserId: default);
//             var journeyInstance = await CreateJourneyInstance(state);
//
//             var authenticationTicket = CreateOneLoginAuthenticationTicket(
//                 vtr: SignInJourneyHelper.AuthenticationOnlyVtr,
//                 sub: user.Subject,
//                 email: user.EmailAddress,
//                 createCoreIdentityVc: false);
//             var callbackResult = await helper.OnOneLoginCallbackAsync(journeyInstance, authenticationTicket);
//             Debug.Assert(callbackResult is ChallengeHttpResult);
//
//             var verifiedTicket = CreateOneLoginAuthenticationTicket(
//                 vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr,
//                 sub: user.Subject,
//                 firstName: firstName,
//                 lastName: lastName,
//                 dateOfBirth: dateOfBirth,
//                 createCoreIdentityVc: true);
//
//             // Act
//             var result = await helper.OnOneLoginCallbackAsync(journeyInstance, verifiedTicket);
//
//             // Assert
//             Assert.NotNull(state.OneLoginAuthenticationTicket);
//             Assert.Null(state.AuthenticationTicket);
//
//             user = await WithDbContextAsync(dbContext => dbContext.OneLoginUsers.SingleAsync(u => u.Subject == user.Subject));
//             Assert.NotEqual(Clock.UtcNow, user.FirstOneLoginSignIn);
//             Assert.Null(user.FirstSignIn);
//             Assert.Equal(Clock.UtcNow, user.LastOneLoginSignIn);
//             Assert.NotEqual(Clock.UtcNow, user.LastSignIn);
//             Assert.Equal(Clock.UtcNow, user.VerifiedOn);
//             Assert.Equal(OneLoginUserVerificationRoute.OneLogin, user.VerificationRoute);
//             Assert.NotNull(user.VerifiedNames);
//             Assert.Collection(user.VerifiedNames, names => Assert.Collection(names, n => Assert.Equal(firstName, n), n => Assert.Equal(lastName, n)));
//             Assert.NotNull(user.VerifiedDatesOfBirth);
//             Assert.Collection(user.VerifiedDatesOfBirth, dob => Assert.Equal(dateOfBirth, dob));
//
//             var redirectResult = Assert.IsType<RedirectHttpResult>(result);
//             Assert.Equal($"/Connect?{journeyInstance.GetUniqueIdQueryParameter()}", redirectResult.Url);
//         });
//
//     [Fact]
//     public Task OnOneLoginCallback_AuthenticationAndVerification_VerificationSucceededAndIdentityUserTrnMatchesVerifiedLastNameAndDateOfBirth_CompletesJourney() =>
//         WithDbContextAsync(async dbContext =>
//         {
//             // Arrange
//             var helper = CreateHelper(dbContext);
//
//             var email = Faker.Internet.Email();
//             var person = await TestData.CreatePersonAsync();
//             var user = await TestData.CreateOneLoginUserAsync(personId: null);
//             Clock.Advance();
//
//             await CreateIdentityUser(person.FirstName, person.LastName, person.Trn, user.EmailAddress!, TrnVerificationLevel.Medium);
//
//             var firstName = person.FirstName;
//             var lastName = person.LastName;
//             var dateOfBirth = person.DateOfBirth;
//
//             var state = new SignInJourneyState(
//                 redirectUri: "/",
//                 serviceName: "Test Service",
//                 serviceUrl: "https://service",
//                 oneLoginAuthenticationScheme: "dummy",
//                 clientApplicationUserId: default);
//             var journeyInstance = await CreateJourneyInstance(state);
//
//             var authenticationTicket = CreateOneLoginAuthenticationTicket(
//                 vtr: SignInJourneyHelper.AuthenticationOnlyVtr,
//                 sub: user.Subject,
//                 email: user.EmailAddress,
//                 createCoreIdentityVc: false);
//             var callbackResult = await helper.OnOneLoginCallbackAsync(journeyInstance, authenticationTicket);
//             Debug.Assert(callbackResult is ChallengeHttpResult);
//
//             var verifiedTicket = CreateOneLoginAuthenticationTicket(
//                 vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr,
//                 sub: user.Subject,
//                 firstName: firstName,
//                 lastName: lastName,
//                 dateOfBirth: dateOfBirth,
//                 createCoreIdentityVc: true);
//
//             // Act
//             var result = await helper.OnOneLoginCallbackAsync(journeyInstance, verifiedTicket);
//
//             // Assert
//             Assert.NotNull(state.AuthenticationTicket);
//
//             user = await WithDbContextAsync(dbContext => dbContext.OneLoginUsers.SingleAsync(u => u.Subject == user.Subject));
//             Assert.NotEqual(Clock.UtcNow, user.FirstOneLoginSignIn);
//             Assert.Equal(Clock.UtcNow, user.LastOneLoginSignIn);
//             Assert.Equal(Clock.UtcNow, user.FirstSignIn);
//             Assert.Equal(Clock.UtcNow, user.LastSignIn);
//             Assert.Equal(OneLoginUserMatchRoute.GetAnIdentityUser, user.MatchRoute);
//
//             var redirectResult = Assert.IsType<RedirectHttpResult>(result);
//             Assert.Equal($"{state.RedirectUri}?{journeyInstance.GetUniqueIdQueryParameter()}", redirectResult.Url);
//         });
//
//     [Fact]
//     public Task OnOneLoginCallback_AuthenticationAndVerification_VerificationSucceededAndIdentityUserWithTrnAssociatedBySupportMatchesVerifiedLastNameAndDateOfBirth_CompletesJourney() =>
//         WithDbContextAsync(async dbContext =>
//         {
//             // Arrange
//             var helper = CreateHelper(dbContext);
//
//             var email = Faker.Internet.Email();
//             var person = await TestData.CreatePersonAsync();
//             var user = await TestData.CreateOneLoginUserAsync(personId: null);
//             Clock.Advance();
//
//             await CreateIdentityUser(person.FirstName, person.LastName, person.Trn, user.EmailAddress!, TrnVerificationLevel.Low, TrnAssociationSource.SupportUi);
//
//             var firstName = person.FirstName;
//             var lastName = person.LastName;
//             var dateOfBirth = person.DateOfBirth;
//
//             var state = new SignInJourneyState(
//                 redirectUri: "/",
//                 serviceName: "Test Service",
//                 serviceUrl: "https://service",
//                 oneLoginAuthenticationScheme: "dummy",
//                 clientApplicationUserId: default);
//             var journeyInstance = await CreateJourneyInstance(state);
//
//             var authenticationTicket = CreateOneLoginAuthenticationTicket(
//                 vtr: SignInJourneyHelper.AuthenticationOnlyVtr,
//                 sub: user.Subject,
//                 email: user.EmailAddress,
//                 createCoreIdentityVc: false);
//             var callbackResult = await helper.OnOneLoginCallbackAsync(journeyInstance, authenticationTicket);
//             Debug.Assert(callbackResult is ChallengeHttpResult);
//
//             var verifiedTicket = CreateOneLoginAuthenticationTicket(
//                 vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr,
//                 sub: user.Subject,
//                 firstName: firstName,
//                 lastName: lastName,
//                 dateOfBirth: dateOfBirth,
//                 createCoreIdentityVc: true);
//
//             // Act
//             var result = await helper.OnOneLoginCallbackAsync(journeyInstance, verifiedTicket);
//
//             // Assert
//             Assert.NotNull(state.AuthenticationTicket);
//
//             user = await WithDbContextAsync(dbContext => dbContext.OneLoginUsers.SingleAsync(u => u.Subject == user.Subject));
//             Assert.NotEqual(Clock.UtcNow, user.FirstOneLoginSignIn);
//             Assert.Equal(Clock.UtcNow, user.LastOneLoginSignIn);
//             Assert.Equal(Clock.UtcNow, user.FirstSignIn);
//             Assert.Equal(Clock.UtcNow, user.LastSignIn);
//             Assert.Equal(OneLoginUserMatchRoute.GetAnIdentityUser, user.MatchRoute);
//
//             var redirectResult = Assert.IsType<RedirectHttpResult>(result);
//             Assert.Equal($"{state.RedirectUri}?{journeyInstance.GetUniqueIdQueryParameter()}", redirectResult.Url);
//         });
//
//     [Fact]
//     public Task OnOneLoginCallback_AuthenticationAndVerification_VerificationSucceededAndIdentityUserWithTrnAssociatedByTrnTokenMatchesVerifiedLastNameAndDateOfBirth_CompletesJourney() =>
//         WithDbContextAsync(async dbContext =>
//         {
//             // Arrange
//             var helper = CreateHelper(dbContext);
//
//             var email = Faker.Internet.Email();
//             var person = await TestData.CreatePersonAsync();
//             var user = await TestData.CreateOneLoginUserAsync(personId: null);
//             Clock.Advance();
//
//             await CreateIdentityUser(person.FirstName, person.LastName, person.Trn, user.EmailAddress!, TrnVerificationLevel.Low, TrnAssociationSource.TrnToken);
//
//             var firstName = person.FirstName;
//             var lastName = person.LastName;
//             var dateOfBirth = person.DateOfBirth;
//
//             var state = new SignInJourneyState(
//                 redirectUri: "/",
//                 serviceName: "Test Service",
//                 serviceUrl: "https://service",
//                 oneLoginAuthenticationScheme: "dummy",
//                 clientApplicationUserId: default);
//             var journeyInstance = await CreateJourneyInstance(state);
//
//             var authenticationTicket = CreateOneLoginAuthenticationTicket(
//                 vtr: SignInJourneyHelper.AuthenticationOnlyVtr,
//                 sub: user.Subject,
//                 email: user.EmailAddress,
//                 createCoreIdentityVc: false);
//             var callbackResult = await helper.OnOneLoginCallbackAsync(journeyInstance, authenticationTicket);
//             Debug.Assert(callbackResult is ChallengeHttpResult);
//
//             var verifiedTicket = CreateOneLoginAuthenticationTicket(
//                 vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr,
//                 sub: user.Subject,
//                 firstName: firstName,
//                 lastName: lastName,
//                 dateOfBirth: dateOfBirth,
//                 createCoreIdentityVc: true);
//
//             // Act
//             var result = await helper.OnOneLoginCallbackAsync(journeyInstance, verifiedTicket);
//
//             // Assert
//             Assert.NotNull(state.AuthenticationTicket);
//
//             user = await WithDbContextAsync(dbContext => dbContext.OneLoginUsers.SingleAsync(u => u.Subject == user.Subject));
//             Assert.NotEqual(Clock.UtcNow, user.FirstOneLoginSignIn);
//             Assert.Equal(Clock.UtcNow, user.LastOneLoginSignIn);
//             Assert.Equal(Clock.UtcNow, user.FirstSignIn);
//             Assert.Equal(Clock.UtcNow, user.LastSignIn);
//             Assert.Equal(OneLoginUserMatchRoute.GetAnIdentityUser, user.MatchRoute);
//
//             var redirectResult = Assert.IsType<RedirectHttpResult>(result);
//             Assert.Equal($"{state.RedirectUri}?{journeyInstance.GetUniqueIdQueryParameter()}", redirectResult.Url);
//         });
//
//     [Fact]
//     public Task OnOneLoginCallback_AuthenticationAndVerification_VerificationSucceededButIdentityUserHasTrnVerificationLevelLow_RedirectsToStartOfMatchingJourney() =>
//         WithDbContextAsync(async dbContext =>
//         {
//             // Arrange
//             var helper = CreateHelper(dbContext);
//
//             var email = Faker.Internet.Email();
//             var person = await TestData.CreatePersonAsync();
//             var user = await TestData.CreateOneLoginUserAsync(personId: null);
//             Clock.Advance();
//
//             await CreateIdentityUser(person.FirstName, person.LastName, person.Trn, user.EmailAddress!, TrnVerificationLevel.Low);
//
//             var firstName = person.FirstName;
//             var lastName = person.LastName;
//             var dateOfBirth = person.DateOfBirth;
//
//             var state = new SignInJourneyState(
//                 redirectUri: "/",
//                 serviceName: "Test Service",
//                 serviceUrl: "https://service",
//                 oneLoginAuthenticationScheme: "dummy",
//                 clientApplicationUserId: default);
//             var journeyInstance = await CreateJourneyInstance(state);
//
//             var authenticationTicket = CreateOneLoginAuthenticationTicket(
//                 vtr: SignInJourneyHelper.AuthenticationOnlyVtr,
//                 sub: user.Subject,
//                 email: user.EmailAddress,
//                 createCoreIdentityVc: false);
//             var callbackResult = await helper.OnOneLoginCallbackAsync(journeyInstance, authenticationTicket);
//             Debug.Assert(callbackResult is ChallengeHttpResult);
//
//             var verifiedTicket = CreateOneLoginAuthenticationTicket(
//                 vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr,
//                 sub: user.Subject,
//                 firstName: firstName,
//                 lastName: lastName,
//                 dateOfBirth: dateOfBirth,
//                 createCoreIdentityVc: true);
//
//             // Act
//             var result = await helper.OnOneLoginCallbackAsync(journeyInstance, verifiedTicket);
//
//             // Assert
//             var redirectResult = Assert.IsType<RedirectHttpResult>(result);
//             Assert.Equal($"/Connect?{journeyInstance.GetUniqueIdQueryParameter()}", redirectResult.Url);
//         });
//
//     [Fact]
//     public Task OnOneLoginCallback_AuthenticationAndVerification_VerificationSucceededButRecordFromIdentityUserTrnDoesNotMatchLastName_RedirectsToStartOfMatchingJourney() =>
//         WithDbContextAsync(async dbContext =>
//         {
//             // Arrange
//             var helper = CreateHelper(dbContext);
//
//             var person = await TestData.CreatePersonAsync();
//             var user = await TestData.CreateOneLoginUserAsync(personId: null);
//             Clock.Advance();
//
//             await CreateIdentityUser(person.FirstName, person.LastName, person.Trn, user.EmailAddress!, TrnVerificationLevel.Medium);
//
//             var firstName = person.FirstName;
//             var lastName = TestData.GenerateChangedLastName([person.FirstName, person.MiddleName, person.LastName]);
//             var dateOfBirth = person.DateOfBirth;
//
//             var state = new SignInJourneyState(
//                 redirectUri: "/",
//                 serviceName: "Test Service",
//                 serviceUrl: "https://service",
//                 oneLoginAuthenticationScheme: "dummy",
//                 clientApplicationUserId: default);
//             var journeyInstance = await CreateJourneyInstance(state);
//
//             var authenticationTicket = CreateOneLoginAuthenticationTicket(
//                 vtr: SignInJourneyHelper.AuthenticationOnlyVtr,
//                 sub: user.Subject,
//                 email: user.EmailAddress,
//                 createCoreIdentityVc: false);
//             var callbackResult = await helper.OnOneLoginCallbackAsync(journeyInstance, authenticationTicket);
//             Debug.Assert(callbackResult is ChallengeHttpResult);
//
//             var verifiedTicket = CreateOneLoginAuthenticationTicket(
//                 vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr,
//                 sub: user.Subject,
//                 firstName: firstName,
//                 lastName: lastName,
//                 dateOfBirth: dateOfBirth,
//                 createCoreIdentityVc: true);
//
//             // Act
//             var result = await helper.OnOneLoginCallbackAsync(journeyInstance, verifiedTicket);
//
//             // Assert
//             var redirectResult = Assert.IsType<RedirectHttpResult>(result);
//             Assert.Equal($"/Connect?{journeyInstance.GetUniqueIdQueryParameter()}", redirectResult.Url);
//         });
//
//     [Fact]
//     public Task OnOneLoginCallback_AuthenticationAndVerification_VerificationSucceededButRecordFromIdentityUserTrnDoesNotMatchDateOfBirth_RedirectsToStartOfMatchingJourney() =>
//         WithDbContextAsync(async dbContext =>
//         {
//             // Arrange
//             var helper = CreateHelper(dbContext);
//
//             var person = await TestData.CreatePersonAsync();
//             var user = await TestData.CreateOneLoginUserAsync(personId: null);
//             Clock.Advance();
//
//             await CreateIdentityUser(person.FirstName, person.LastName, person.Trn, user.EmailAddress!, TrnVerificationLevel.Medium);
//
//             var firstName = person.FirstName;
//             var lastName = person.LastName;
//             var dateOfBirth = person.DateOfBirth.AddDays(1);
//
//             var state = new SignInJourneyState(
//                 redirectUri: "/",
//                 serviceName: "Test Service",
//                 serviceUrl: "https://service",
//                 oneLoginAuthenticationScheme: "dummy",
//                 clientApplicationUserId: default);
//             var journeyInstance = await CreateJourneyInstance(state);
//
//             var authenticationTicket = CreateOneLoginAuthenticationTicket(
//                 vtr: SignInJourneyHelper.AuthenticationOnlyVtr,
//                 sub: user.Subject,
//                 email: user.EmailAddress,
//                 createCoreIdentityVc: false);
//             var callbackResult = await helper.OnOneLoginCallbackAsync(journeyInstance, authenticationTicket);
//             Debug.Assert(callbackResult is ChallengeHttpResult);
//
//             var verifiedTicket = CreateOneLoginAuthenticationTicket(
//                 vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr,
//                 sub: user.Subject,
//                 firstName: firstName,
//                 lastName: lastName,
//                 dateOfBirth: dateOfBirth,
//                 createCoreIdentityVc: true);
//
//             // Act
//             var result = await helper.OnOneLoginCallbackAsync(journeyInstance, verifiedTicket);
//
//             // Assert
//             var redirectResult = Assert.IsType<RedirectHttpResult>(result);
//             Assert.Equal($"/Connect?{journeyInstance.GetUniqueIdQueryParameter()}", redirectResult.Url);
//         });
//
//     [Fact]
//     public Task OnOneLoginCallback_AuthenticationAndVerification_VerificationSucceededAndTrnTokenMatchesVerifiedLastNameAndDateOfBirth_CompletesJourney() =>
//         WithDbContextAsync(async dbContext =>
//         {
//             // Arrange
//             var helper = CreateHelper(dbContext);
//
//             var person = await TestData.CreatePersonAsync();
//             var user = await TestData.CreateOneLoginUserAsync(personId: null);
//             Clock.Advance();
//
//             var trnToken = await CreateTrnToken(person.Trn, user.EmailAddress!);
//
//             var firstName = person.FirstName;
//             var lastName = person.LastName;
//             var dateOfBirth = person.DateOfBirth;
//
//             var state = new SignInJourneyState(
//                 redirectUri: "/",
//                 serviceName: "Test Service",
//                 serviceUrl: "https://service",
//                 oneLoginAuthenticationScheme: "dummy",
//                 clientApplicationUserId: default,
//                 trnToken);
//             var journeyInstance = await CreateJourneyInstance(state);
//
//             var authenticationTicket = CreateOneLoginAuthenticationTicket(
//                 vtr: SignInJourneyHelper.AuthenticationOnlyVtr,
//                 sub: user.Subject,
//                 email: user.EmailAddress,
//                 createCoreIdentityVc: false);
//             var callbackResult = await helper.OnOneLoginCallbackAsync(journeyInstance, authenticationTicket);
//             Debug.Assert(callbackResult is ChallengeHttpResult);
//
//             var verifiedTicket = CreateOneLoginAuthenticationTicket(
//                 vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr,
//                 sub: user.Subject,
//                 firstName: firstName,
//                 lastName: lastName,
//                 dateOfBirth: dateOfBirth,
//                 createCoreIdentityVc: true);
//
//             // Act
//             var result = await helper.OnOneLoginCallbackAsync(journeyInstance, verifiedTicket);
//
//             // Assert
//             Assert.NotNull(state.AuthenticationTicket);
//
//             user = await WithDbContextAsync(dbContext => dbContext.OneLoginUsers.SingleAsync(u => u.Subject == user.Subject));
//             Assert.NotEqual(Clock.UtcNow, user.FirstOneLoginSignIn);
//             Assert.Equal(Clock.UtcNow, user.LastOneLoginSignIn);
//             Assert.Equal(Clock.UtcNow, user.FirstSignIn);
//             Assert.Equal(Clock.UtcNow, user.LastSignIn);
//             Assert.Equal(OneLoginUserMatchRoute.TrnToken, user.MatchRoute);
//
//             var redirectResult = Assert.IsType<RedirectHttpResult>(result);
//             Assert.Equal($"{state.RedirectUri}?{journeyInstance.GetUniqueIdQueryParameter()}", redirectResult.Url);
//         });
//
//     [Fact]
//     public Task OnOneLoginCallback_AuthenticationAndVerification_VerificationSucceededButTrnTokenDoesNotExist_RedirectsToStartOfMatchingJourney() =>
//         WithDbContextAsync(async dbContext =>
//         {
//             // Arrange
//             var helper = CreateHelper(dbContext);
//
//             var person = await TestData.CreatePersonAsync();
//             var user = await TestData.CreateOneLoginUserAsync(personId: null);
//             Clock.Advance();
//
//             var trnToken = Guid.NewGuid().ToString();
//
//             var firstName = person.FirstName;
//             var lastName = person.LastName;
//             var dateOfBirth = person.DateOfBirth;
//
//             var state = new SignInJourneyState(
//                 redirectUri: "/",
//                 serviceName: "Test Service",
//                 serviceUrl: "https://service",
//                 oneLoginAuthenticationScheme: "dummy",
//                 clientApplicationUserId: default,
//                 trnToken);
//             var journeyInstance = await CreateJourneyInstance(state);
//
//             var authenticationTicket = CreateOneLoginAuthenticationTicket(
//                 vtr: SignInJourneyHelper.AuthenticationOnlyVtr,
//                 sub: user.Subject,
//                 email: user.EmailAddress,
//                 createCoreIdentityVc: false);
//             var callbackResult = await helper.OnOneLoginCallbackAsync(journeyInstance, authenticationTicket);
//             Debug.Assert(callbackResult is ChallengeHttpResult);
//
//             var verifiedTicket = CreateOneLoginAuthenticationTicket(
//                 vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr,
//                 sub: user.Subject,
//                 firstName: firstName,
//                 lastName: lastName,
//                 dateOfBirth: dateOfBirth,
//                 createCoreIdentityVc: true);
//
//             // Act
//             var result = await helper.OnOneLoginCallbackAsync(journeyInstance, verifiedTicket);
//
//             // Assert
//             var redirectResult = Assert.IsType<RedirectHttpResult>(result);
//             Assert.Equal($"/Connect?{journeyInstance.GetUniqueIdQueryParameter()}", redirectResult.Url);
//         });
//
//     [Fact]
//     public Task OnOneLoginCallback_AuthenticationAndVerification_VerificationSucceededButTrnTokenHasExpired_RedirectsToStartOfMatchingJourney() =>
//         WithDbContextAsync(async dbContext =>
//         {
//             // Arrange
//             var helper = CreateHelper(dbContext);
//
//             var person = await TestData.CreatePersonAsync();
//             var user = await TestData.CreateOneLoginUserAsync(personId: null);
//             Clock.Advance();
//
//             var trnToken = await CreateTrnToken(person.Trn, user.EmailAddress!, expires: TimeSpan.FromSeconds(-1));
//
//             var firstName = person.FirstName;
//             var lastName = person.LastName;
//             var dateOfBirth = person.DateOfBirth;
//
//             var state = new SignInJourneyState(
//                 redirectUri: "/",
//                 serviceName: "Test Service",
//                 serviceUrl: "https://service",
//                 oneLoginAuthenticationScheme: "dummy",
//                 clientApplicationUserId: default,
//                 trnToken);
//             var journeyInstance = await CreateJourneyInstance(state);
//
//             var authenticationTicket = CreateOneLoginAuthenticationTicket(
//                 vtr: SignInJourneyHelper.AuthenticationOnlyVtr,
//                 sub: user.Subject,
//                 email: user.EmailAddress,
//                 createCoreIdentityVc: false);
//             var callbackResult = await helper.OnOneLoginCallbackAsync(journeyInstance, authenticationTicket);
//             Debug.Assert(callbackResult is ChallengeHttpResult);
//
//             var verifiedTicket = CreateOneLoginAuthenticationTicket(
//                 vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr,
//                 sub: user.Subject,
//                 firstName: firstName,
//                 lastName: lastName,
//                 dateOfBirth: dateOfBirth,
//                 createCoreIdentityVc: true);
//
//             // Act
//             var result = await helper.OnOneLoginCallbackAsync(journeyInstance, verifiedTicket);
//
//             // Assert
//             var redirectResult = Assert.IsType<RedirectHttpResult>(result);
//             Assert.Equal($"/Connect?{journeyInstance.GetUniqueIdQueryParameter()}", redirectResult.Url);
//         });
//
//     [Fact]
//     public Task OnOneLoginCallback_AuthenticationAndVerification_VerificationSucceededButTrnTokenHasAlreadyBeenUsed_RedirectsToStartOfMatchingJourney() =>
//         WithDbContextAsync(async dbContext =>
//         {
//             // Arrange
//             var helper = CreateHelper(dbContext);
//
//             var person = await TestData.CreatePersonAsync();
//             var user = await TestData.CreateOneLoginUserAsync(personId: null);
//             Clock.Advance();
//
//             var trnToken = await CreateTrnToken(person.Trn, user.EmailAddress!, userId: Guid.NewGuid());
//
//             var state = new SignInJourneyState(
//                 redirectUri: "/",
//                 serviceName: "Test Service",
//                 serviceUrl: "https://service",
//                 oneLoginAuthenticationScheme: "dummy",
//                 clientApplicationUserId: default,
//                 trnToken);
//             var journeyInstance = await CreateJourneyInstance(state);
//
//             var authenticationTicket = CreateOneLoginAuthenticationTicket(
//                 vtr: SignInJourneyHelper.AuthenticationOnlyVtr,
//                 sub: user.Subject,
//                 email: user.EmailAddress,
//                 createCoreIdentityVc: false);
//             var callbackResult = await helper.OnOneLoginCallbackAsync(journeyInstance, authenticationTicket);
//             Debug.Assert(callbackResult is ChallengeHttpResult);
//
//             var verifiedTicket = CreateOneLoginAuthenticationTicket(
//                 vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr,
//                 sub: user.Subject,
//                 firstName: person.FirstName,
//                 lastName: person.LastName,
//                 dateOfBirth: person.DateOfBirth,
//                 createCoreIdentityVc: true);
//
//             // Act
//             var result = await helper.OnOneLoginCallbackAsync(journeyInstance, verifiedTicket);
//
//             // Assert
//             var redirectResult = Assert.IsType<RedirectHttpResult>(result);
//             Assert.Equal($"/Connect?{journeyInstance.GetUniqueIdQueryParameter()}", redirectResult.Url);
//         });
//
//     [Fact]
//     public Task OnOneLoginCallback_AuthenticationAndVerification_VerificationSucceededButRecordFromTrnTokenDoesNotMatchLastName_RedirectsToStartOfMatchingJourney() =>
//         WithDbContextAsync(async dbContext =>
//         {
//             // Arrange
//             var helper = CreateHelper(dbContext);
//
//             var person = await TestData.CreatePersonAsync();
//             var user = await TestData.CreateOneLoginUserAsync(personId: null);
//             Clock.Advance();
//
//             var trnToken = await CreateTrnToken(person.Trn, user.EmailAddress!);
//
//             var firstName = person.FirstName;
//             var lastName = TestData.GenerateChangedLastName([person.FirstName, person.MiddleName, person.LastName]);
//             var dateOfBirth = person.DateOfBirth;
//
//             var state = new SignInJourneyState(
//                 redirectUri: "/",
//                 serviceName: "Test Service",
//                 serviceUrl: "https://service",
//                 oneLoginAuthenticationScheme: "dummy",
//                 clientApplicationUserId: default,
//                 trnToken);
//             var journeyInstance = await CreateJourneyInstance(state);
//
//             var authenticationTicket = CreateOneLoginAuthenticationTicket(
//                 vtr: SignInJourneyHelper.AuthenticationOnlyVtr,
//                 sub: user.Subject,
//                 email: user.EmailAddress,
//                 createCoreIdentityVc: false);
//             var callbackResult = await helper.OnOneLoginCallbackAsync(journeyInstance, authenticationTicket);
//             Debug.Assert(callbackResult is ChallengeHttpResult);
//
//             var verifiedTicket = CreateOneLoginAuthenticationTicket(
//                 vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr,
//                 sub: user.Subject,
//                 firstName: firstName,
//                 lastName: lastName,
//                 dateOfBirth: dateOfBirth,
//                 createCoreIdentityVc: true);
//
//             // Act
//             var result = await helper.OnOneLoginCallbackAsync(journeyInstance, verifiedTicket);
//
//             // Assert
//             var redirectResult = Assert.IsType<RedirectHttpResult>(result);
//             Assert.Equal($"/Connect?{journeyInstance.GetUniqueIdQueryParameter()}", redirectResult.Url);
//         });
//
//     [Fact]
//     public Task OnOneLoginCallback_AuthenticationAndVerification_VerificationSucceededButRecordFromTrnTokenDoesNotMatchDateOfBirth_RedirectsToStartOfMatchingJourney() =>
//         WithDbContextAsync(async dbContext =>
//         {
//             // Arrange
//             var helper = CreateHelper(dbContext);
//
//             var person = await TestData.CreatePersonAsync();
//             var user = await TestData.CreateOneLoginUserAsync(personId: null);
//             Clock.Advance();
//
//             var trnToken = await CreateTrnToken(person.Trn, user.EmailAddress!);
//
//             var firstName = person.FirstName;
//             var lastName = person.LastName;
//             var dateOfBirth = person.DateOfBirth.AddDays(1);
//
//             var state = new SignInJourneyState(
//                 redirectUri: "/",
//                 serviceName: "Test Service",
//                 serviceUrl: "https://service",
//                 oneLoginAuthenticationScheme: "dummy",
//                 clientApplicationUserId: default,
//                 trnToken);
//             var journeyInstance = await CreateJourneyInstance(state);
//
//             var authenticationTicket = CreateOneLoginAuthenticationTicket(
//                 vtr: SignInJourneyHelper.AuthenticationOnlyVtr,
//                 sub: user.Subject,
//                 email: user.EmailAddress,
//                 createCoreIdentityVc: false);
//             var callbackResult = await helper.OnOneLoginCallbackAsync(journeyInstance, authenticationTicket);
//             Debug.Assert(callbackResult is ChallengeHttpResult);
//
//             var verifiedTicket = CreateOneLoginAuthenticationTicket(
//                 vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr,
//                 sub: user.Subject,
//                 firstName: firstName,
//                 lastName: lastName,
//                 dateOfBirth: dateOfBirth,
//                 createCoreIdentityVc: true);
//
//             // Act
//             var result = await helper.OnOneLoginCallbackAsync(journeyInstance, verifiedTicket);
//
//             // Assert
//             var redirectResult = Assert.IsType<RedirectHttpResult>(result);
//             Assert.Equal($"/Connect?{journeyInstance.GetUniqueIdQueryParameter()}", redirectResult.Url);
//         });
//
//     [Fact]
//     public Task TryMatchToTeachingRecord_MatchesZeroResults_ReturnsFalseAndDoesNotSetAuthenticationTicket() =>
//         WithDbContextAsync(async dbContext =>
//         {
//             // Arrange
//             var verifiedFirstName = Faker.Name.Last();
//             var verifiedLastName = Faker.Name.Last();
//             var verifiedDateOfBirth = DateOnly.FromDateTime(Faker.Identification.DateOfBirth());
//             var user = await TestData.CreateOneLoginUserAsync(personId: null, verifiedInfo: ([verifiedFirstName, verifiedLastName], verifiedDateOfBirth));
//             Clock.Advance();
//
//             var state = new SignInJourneyState(
//                 redirectUri: "/",
//                 serviceName: "Test Service",
//                 serviceUrl: "https://service",
//                 oneLoginAuthenticationScheme: "dummy",
//                 clientApplicationUserId: default);
//             var journeyInstance = await CreateJourneyInstance(state);
//
//             var helper = CreateHelper(
//                 dbContext,
//                 m => m
//                     .Setup(mock => mock.MatchPersonAsync(It.Is<MatchPersonOptions>(r =>
//                         r.Names.SequenceEqual(state.VerifiedNames!) &&
//                         r.DatesOfBirth.SequenceEqual(state.VerifiedDatesOfBirth!) &&
//                         r.NationalInsuranceNumber == state.NationalInsuranceNumber &&
//                         r.Trn == state.Trn)))
//                     .ReturnsAsync(value: null));
//
//             var authenticationTicket = CreateOneLoginAuthenticationTicket(
//                 vtr: SignInJourneyHelper.AuthenticationOnlyVtr,
//                 sub: user.Subject,
//                 createCoreIdentityVc: false);
//             await helper.OnOneLoginCallbackAsync(journeyInstance,
//                 authenticationTicket);
//
//             await journeyInstance.UpdateStateAsync(state => state.SetNationalInsuranceNumber(true,
//                 Faker.Identification.UkNationalInsuranceNumber()));
//
//             // Act
//             var result = await helper.TryMatchToTeachingRecordAsync(journeyInstance);
//
//             // Assert
//             Assert.False(result);
//
//             user = await dbContext.OneLoginUsers.SingleAsync(u => u.Subject == user.Subject);
//             Assert.Null(user.PersonId);
//
//             Assert.Null(state.AuthenticationTicket);
//         });
//
//     [Fact]
//     public Task TryMatchToTeachingRecord_Matches_ReturnsTrueAndUpdatesOneLoginUserAssignsAuthenticationTicket() =>
//         WithDbContextAsync(async dbContext =>
//         {
//             // Arrange
//             var firstName = Faker.Name.Last();
//             var lastName = Faker.Name.Last();
//             var dateOfBirth = DateOnly.FromDateTime(Faker.Identification.DateOfBirth());
//             var person = await TestData.CreatePersonAsync(p => p.WithFirstName(firstName).WithLastName(lastName).WithDateOfBirth(dateOfBirth).WithNationalInsuranceNumber());
//
//             var user = await TestData.CreateOneLoginUserAsync(personId: null, verifiedInfo: ([firstName, lastName], dateOfBirth));
//             Clock.Advance();
//
//             var state = new SignInJourneyState(
//                 redirectUri: "/",
//                 serviceName: "Test Service",
//                 serviceUrl: "https://service",
//                 oneLoginAuthenticationScheme: "dummy",
//                 clientApplicationUserId: default);
//             var journeyInstance = await CreateJourneyInstance(state);
//
//             var helper = CreateHelper(
//                 dbContext,
//                 m => m
//                     .Setup(mock => mock.MatchPersonAsync(It.Is<MatchPersonOptions>(r =>
//                         r.Names.SequenceEqual(state.VerifiedNames!) &&
//                         r.DatesOfBirth.SequenceEqual(state.VerifiedDatesOfBirth!) &&
//                         r.NationalInsuranceNumber == state.NationalInsuranceNumber &&
//                         r.Trn == state.Trn)))
//                     .ReturnsAsync(new MatchPersonResult(
//                         person.PersonId,
//                         person.Trn,
//                         new Dictionary<PersonMatchedAttribute, string>()
//                         {
//                             { PersonMatchedAttribute.FullName, $"{person.FirstName} {person.LastName}" },
//                             { PersonMatchedAttribute.NationalInsuranceNumber, person.NationalInsuranceNumber! }
//                         })));
//
//             var authenticationTicket = CreateOneLoginAuthenticationTicket(
//                 vtr: SignInJourneyHelper.AuthenticationOnlyVtr,
//                 sub: user.Subject,
//                 createCoreIdentityVc: false);
//             await helper.OnOneLoginCallbackAsync(journeyInstance, authenticationTicket);
//
//             await journeyInstance.UpdateStateAsync(state => state.SetNationalInsuranceNumber(true, person.NationalInsuranceNumber));
//
//             // Act
//             var result = await helper.TryMatchToTeachingRecordAsync(journeyInstance);
//
//             // Assert
//             Assert.True(result);
//
//             user = await dbContext.OneLoginUsers.SingleAsync(u => u.Subject == user.Subject);
//             Assert.Equal(Clock.UtcNow, user.FirstSignIn);
//             Assert.Equal(Clock.UtcNow, user.LastSignIn);
//             Assert.Equal(person.PersonId, user.PersonId);
//             Assert.Equal(OneLoginUserMatchRoute.Interactive, user.MatchRoute);
//             Assert.NotNull(user.MatchedAttributes);
//             Assert.NotEmpty(user.MatchedAttributes);
//
//             Assert.NotNull(state.AuthenticationTicket);
//             Assert.Equal(person.Trn, state.AuthenticationTicket.Principal.FindFirstValue(ClaimTypes.Trn));
//         });
//
//     private SignInJourneyHelper CreateHelper(TrsDbContext dbContext, Action<Mock<OneLoginService>>? configureOneLoginServiceMock = null)
//     {
//         var linkGenerator = new FakeLinkGenerator();
//
//         using var rsa = RSA.Create(keySizeInBits: 2048);
//
//         var options = Options.Create(new AuthorizeAccessOptions
//         {
//             ShowDebugPages = false,
//             OneLoginSigningKeys =
//             [
//                 new AuthorizeAccessOptionsOneLoginSigningKey
//                 {
//                     KeyId = "test-key-id",
//                     PrivateKeyPem = rsa.ExportRSAPrivateKeyPem()
//                 }
//             ]
//         });
//
//         var oneLoginServiceMock = new Mock<OneLoginService>(dbContext, Mock.Of<INotificationSender>(), Mock.Of<IBackgroundJobScheduler>())
//         {
//             CallBase = true
//         };
//         configureOneLoginServiceMock?.Invoke(oneLoginServiceMock);
//         var oneLoginService = oneLoginServiceMock.Object;
//
//         return ActivatorUtilities.CreateInstance<SignInJourneyHelper>(HostFixture.Services, dbContext, oneLoginService, linkGenerator, options, Clock);
//     }
//
//     private async Task CreateIdentityUser(string firstName, string lastName, string trn, string email, TrnVerificationLevel trnVerificationLevel, TrnAssociationSource trnAssociationSource = TrnAssociationSource.Lookup)
//     {
//         using (var idDbContext = HostFixture.Services.GetRequiredService<IdDbContext>())
//         {
//             idDbContext.Users.Add(new User()
//             {
//                 UserId = Guid.NewGuid(),
//                 EmailAddress = email,
//                 FirstName = firstName,
//                 LastName = lastName,
//                 Created = Clock.UtcNow,
//                 Updated = Clock.UtcNow,
//                 UserType = IdModelTypes.UserType.Teacher,
//                 TrnVerificationLevel = trnVerificationLevel,
//                 TrnAssociationSource = trnAssociationSource,
//                 Trn = trn
//             });
//
//             await idDbContext.SaveChangesAsync();
//         }
//     }
//
//     private async Task<string> CreateTrnToken(string trn, string email, TimeSpan? expires = null, Guid? userId = null)
//     {
//         var trnToken = Guid.NewGuid().ToString();
//         var expiresUtc = Clock.UtcNow.Add(expires ?? TimeSpan.FromHours(1));
//
//         using (var idDbContext = HostFixture.Services.GetRequiredService<IdDbContext>())
//         {
//             idDbContext.TrnTokens.Add(new IdTrnToken()
//             {
//                 TrnToken = trnToken,
//                 Trn = trn,
//                 CreatedUtc = Clock.UtcNow,
//                 ExpiresUtc = expiresUtc,
//                 Email = email,
//                 UserId = userId
//             });
//
//             await idDbContext.SaveChangesAsync();
//         }
//
//         return trnToken;
//     }
//
//     private class FakeLinkGenerator : AuthorizeAccessLinkGenerator
//     {
//         protected override string GetRequiredPathByPage(string page, string? handler = null, object? routeValues = null) => page;
//     }
// }
