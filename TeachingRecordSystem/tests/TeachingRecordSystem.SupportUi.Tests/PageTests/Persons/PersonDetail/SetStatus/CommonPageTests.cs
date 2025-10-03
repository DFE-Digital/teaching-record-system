// using AngleSharp.Html.Dom;
// using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;
//
// namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.SetStatus;
//
// [NotInParallel]
// public class CommonPageTests(HostFixture hostFixture) : SetStatusTestBase(hostFixture)
// {
//     [Test]
//     [MatrixDataSource]
//     public async Task UserDoesNotHavePermission_ReturnsForbidden(
//         [Matrix("change-reason", "check-answers")] string page,
//         [Matrix(PersonStatus.Deactivated, PersonStatus.Active)] PersonStatus targetStatus,
//         [Matrix(UserRoles.Viewer, UserRoles.AlertsManagerTra, UserRoles.AlertsManagerTraDbs, null)] string? role,
//         [MatrixHttpMethods(TestHttpMethods.GetAndPost)] HttpMethod httpMethod)
//     {
//         // Arrange
//         SetCurrentUser(TestUsers.GetUser(role));
//
//         var person = await CreatePersonToBecomeStatus(targetStatus);
//
//         var stateBuilder = new SetStatusStateBuilder()
//             .WithInitializedState()
//             .WithUploadEvidenceChoice(false);
//
//         if (targetStatus == PersonStatus.Deactivated)
//         {
//             stateBuilder.WithDeactivateReasonChoice(DeactivateReasonOption.ProblemWithTheRecord);
//         }
//         else
//         {
//             stateBuilder.WithReactivateReasonChoice(ReactivateReasonOption.DeactivatedByMistake);
//         }
//
//         var journeyInstance = await CreateJourneyInstanceAsync(
//             person.PersonId,
//             stateBuilder.Build());
//
//         // Act
//         var request = new HttpRequestMessage(httpMethod, GetRequestPath(person, targetStatus, page, journeyInstance));
//
//         // Act
//         var response = await HttpClient.SendAsync(request);
//
//         // Assert
//         Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
//     }
//
//     [Test]
//     [MatrixDataSource]
//     public async Task PersonIsAlreadyTargetStatus_ReturnsBadRequest(
//         [Matrix("change-reason", "check-answers")] string page,
//         [Matrix(PersonStatus.Deactivated, PersonStatus.Active)] PersonStatus targetStatus,
//         [MatrixHttpMethods(TestHttpMethods.GetAndPost)] HttpMethod httpMethod)
//     {
//         // Arrange
//         var person = await CreatePersonWithCurrentStatus(targetStatus);
//
//         var stateBuilder = new SetStatusStateBuilder()
//             .WithInitializedState()
//             .WithUploadEvidenceChoice(false);
//
//         if (targetStatus == PersonStatus.Deactivated)
//         {
//             stateBuilder.WithDeactivateReasonChoice(DeactivateReasonOption.ProblemWithTheRecord);
//         }
//         else
//         {
//             stateBuilder.WithReactivateReasonChoice(ReactivateReasonOption.DeactivatedByMistake);
//         }
//
//         var journeyInstance = await CreateJourneyInstanceAsync(
//             person.PersonId,
//             stateBuilder.Build());
//
//         // Act
//         var request = new HttpRequestMessage(httpMethod, GetRequestPath(person, targetStatus, page, journeyInstance));
//
//         // Act
//         var response = await HttpClient.SendAsync(request);
//
//         // Assert
//         Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
//     }
//
//     [Test]
//     [MatrixDataSource]
//     public async Task TargetStatusActive_PersonWasDeactivatedAsPartOfAMerge_ReturnsBadRequest(
//         [Matrix("change-reason", "check-answers")] string page,
//         [MatrixHttpMethods(TestHttpMethods.GetAndPost)] HttpMethod httpMethod)
//     {
//         // Arrange
//         var primaryPerson = await TestData.CreatePersonAsync(p => p
//             .WithPersonDataSource(TestDataPersonDataSource.Trs));
//         var secondaryPerson = await CreatePersonWithCurrentStatus(PersonStatus.Deactivated, p => p
//             .WithMergedWithPersonId(primaryPerson.PersonId));
//
//         var stateBuilder = new SetStatusStateBuilder()
//             .WithInitializedState()
//             .WithUploadEvidenceChoice(false)
//             .WithReactivateReasonChoice(ReactivateReasonOption.DeactivatedByMistake);
//
//         var journeyInstance = await CreateJourneyInstanceAsync(
//             secondaryPerson.PersonId,
//             stateBuilder.Build());
//
//         // Act
//         var request = new HttpRequestMessage(httpMethod, GetRequestPath(secondaryPerson, PersonStatus.Active, page, journeyInstance));
//
//         // Act
//         var response = await HttpClient.SendAsync(request);
//
//         // Assert
//         Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
//     }
//
//     [Test]
//     [MatrixDataSource]
//     public async Task ReasonNotSet_RedirectsToChangeReason(
//         [Matrix("check-answers")] string page,
//         [Matrix(PersonStatus.Deactivated, PersonStatus.Active)] PersonStatus targetStatus,
//         [MatrixHttpMethods((TestHttpMethods.GetAndPost))] HttpMethod httpMethod)
//     {
//         var person = await CreatePersonToBecomeStatus(targetStatus);
//
//         var journeyInstance = await CreateJourneyInstanceAsync(
//             person.PersonId,
//             new SetStatusStateBuilder()
//             .WithInitializedState()
//             .WithUploadEvidenceChoice(false)
//             .Build());
//
//         // Act
//         var request = new HttpRequestMessage(httpMethod, GetRequestPath(person, targetStatus, page, journeyInstance));
//         var response = await HttpClient.SendAsync(request);
//
//         // Assert
//         AssertEx.ResponseIsRedirectTo(response,
//             $"/persons/{person.PersonId}/set-status/{targetStatus}/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");
//     }
//
//     [Test]
//     [MatrixDataSource]
//     public async Task ReasonSetToAnotherReason_ButReasonDetailNotProvided_RedirectsToChangeReason(
//         [Matrix("check-answers")] string page,
//         [Matrix(PersonStatus.Deactivated, PersonStatus.Active)] PersonStatus targetStatus,
//         [MatrixHttpMethods(TestHttpMethods.GetAndPost)] HttpMethod httpMethod)
//     {
//         var person = await CreatePersonToBecomeStatus(targetStatus);
//
//         var stateBuilder = new SetStatusStateBuilder()
//             .WithInitializedState()
//             .WithUploadEvidenceChoice(false);
//
//         if (targetStatus == PersonStatus.Deactivated)
//         {
//             stateBuilder.WithDeactivateReasonChoice(DeactivateReasonOption.AnotherReason, detailText: null);
//         }
//         else
//         {
//             stateBuilder.WithReactivateReasonChoice(ReactivateReasonOption.AnotherReason, detailText: null);
//         }
//
//         var journeyInstance = await CreateJourneyInstanceAsync(
//             person.PersonId,
//             stateBuilder.Build());
//
//         // Act
//         var request = new HttpRequestMessage(httpMethod, GetRequestPath(person, targetStatus, page, journeyInstance));
//         var response = await HttpClient.SendAsync(request);
//
//         // Assert
//         AssertEx.ResponseIsRedirectTo(response,
//             $"/persons/{person.PersonId}/set-status/{targetStatus}/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");
//     }
//
//     [Test]
//     [MatrixDataSource]
//     public async Task UploadEvidenceNotSet_RedirectsToChangeReason(
//         [Matrix("check-answers")] string page,
//         [Matrix(PersonStatus.Deactivated, PersonStatus.Active)] PersonStatus targetStatus,
//         [MatrixHttpMethods(TestHttpMethods.GetAndPost)] HttpMethod httpMethod)
//     {
//         var person = await CreatePersonToBecomeStatus(targetStatus);
//
//         var stateBuilder = new SetStatusStateBuilder()
//             .WithInitializedState();
//
//         if (targetStatus == PersonStatus.Deactivated)
//         {
//             stateBuilder.WithDeactivateReasonChoice(DeactivateReasonOption.ProblemWithTheRecord);
//         }
//         else
//         {
//             stateBuilder.WithReactivateReasonChoice(ReactivateReasonOption.DeactivatedByMistake);
//         }
//
//         var journeyInstance = await CreateJourneyInstanceAsync(
//             person.PersonId,
//             stateBuilder.Build());
//
//         // Act
//         var request = new HttpRequestMessage(httpMethod, GetRequestPath(person, targetStatus, page, journeyInstance));
//         var response = await HttpClient.SendAsync(request);
//
//         // Assert
//         AssertEx.ResponseIsRedirectTo(response,
//             $"/persons/{person.PersonId}/set-status/{targetStatus}/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");
//     }
//
//     [Test]
//     [MatrixDataSource]
//     public async Task UploadEvidenceSetToTrue_ButEvidenceFileNotUploaded_RedirectsToChangeReason(
//         [Matrix("check-answers")] string page,
//         [Matrix(PersonStatus.Deactivated, PersonStatus.Active)] PersonStatus targetStatus,
//         [MatrixHttpMethods(TestHttpMethods.GetAndPost)] HttpMethod httpMethod)
//     {
//         var person = await CreatePersonToBecomeStatus(targetStatus);
//
//         var stateBuilder = new SetStatusStateBuilder()
//             .WithInitializedState()
//             .WithUploadEvidenceChoice(true, evidenceFileId: null);
//
//         if (targetStatus == PersonStatus.Deactivated)
//         {
//             stateBuilder.WithDeactivateReasonChoice(DeactivateReasonOption.ProblemWithTheRecord);
//         }
//         else
//         {
//             stateBuilder.WithReactivateReasonChoice(ReactivateReasonOption.DeactivatedByMistake);
//         }
//
//         var journeyInstance = await CreateJourneyInstanceAsync(
//             person.PersonId,
//             stateBuilder.Build());
//
//         // Act
//         var request = new HttpRequestMessage(httpMethod, GetRequestPath(person, targetStatus, page, journeyInstance));
//
//         // Act
//         var response = await HttpClient.SendAsync(request);
//
//         // Assert
//         AssertEx.ResponseIsRedirectTo(response,
//             $"/persons/{person.PersonId}/set-status/{targetStatus}/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");
//     }
//
//     [Test]
//     [MatrixDataSource]
//     public async Task Get_PageTitle_CaptionIsExpected(
//         [Matrix("change-reason", "check-answers")] string page,
//         [Matrix(PersonStatus.Deactivated, PersonStatus.Active)] PersonStatus targetStatus)
//     {
//         // Arrange
//         var person = await CreatePersonToBecomeStatus(targetStatus, p => p
//             .WithFirstName("Lily")
//             .WithMiddleName("The")
//             .WithLastName("Pink"));
//
//         var stateBuilder = new SetStatusStateBuilder()
//             .WithInitializedState()
//             .WithUploadEvidenceChoice(false);
//
//         if (targetStatus == PersonStatus.Deactivated)
//         {
//             stateBuilder.WithDeactivateReasonChoice(DeactivateReasonOption.ProblemWithTheRecord);
//         }
//         else
//         {
//             stateBuilder.WithReactivateReasonChoice(ReactivateReasonOption.DeactivatedByMistake);
//         }
//
//         var journeyInstance = await CreateJourneyInstanceAsync(
//             person.PersonId,
//             stateBuilder.Build());
//
//         var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, targetStatus, page, journeyInstance));
//
//         // Act
//         var response = await HttpClient.SendAsync(request);
//
//         // Assert
//         var doc = await AssertEx.HtmlResponseAsync(response);
//         var caption = doc.GetElementByTestId("set-status-caption");
//
//         if (targetStatus == PersonStatus.Deactivated)
//         {
//             Assert.Equal("Deactivate record - Lily The Pink", caption!.TrimmedText());
//         }
//         else
//         {
//             Assert.Equal("Reactivate record - Lily The Pink", caption!.TrimmedText());
//         }
//     }
//
//     [Test]
//     [MethodDataSource(nameof(AllCombinationsOf),
//         "change-reason", null, new[] { PersonStatus.Active, PersonStatus.Deactivated })]
//     [MethodDataSource(nameof(AllCombinationsOf),
//         "check-answers", "change-reason", new[] { PersonStatus.Active, PersonStatus.Deactivated })]
//     public async Task Get_BacklinkLinksToExpected(string page, string? expectedPage, PersonStatus targetStatus)
//     {
//         var person = await CreatePersonToBecomeStatus(targetStatus);
//
//         var stateBuilder = new SetStatusStateBuilder()
//             .WithInitializedState()
//             .WithUploadEvidenceChoice(false);
//
//         if (targetStatus == PersonStatus.Deactivated)
//         {
//             stateBuilder.WithDeactivateReasonChoice(DeactivateReasonOption.ProblemWithTheRecord);
//         }
//         else
//         {
//             stateBuilder.WithReactivateReasonChoice(ReactivateReasonOption.DeactivatedByMistake);
//         }
//
//         var journeyInstance = await CreateJourneyInstanceAsync(
//             person.PersonId,
//             stateBuilder.Build());
//
//         // Act
//         var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, targetStatus, page, journeyInstance));
//         var response = await HttpClient.SendAsync(request);
//
//         // Assert
//         Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
//         var document = await response.GetDocumentAsync();
//         var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
//         Assert.NotNull(backlink);
//         var expectedBackLink = $"/persons/{person.PersonId}";
//         if (expectedPage is not null)
//         {
//             expectedBackLink += $"/set-status/{targetStatus}/{expectedPage}?{journeyInstance?.GetUniqueIdQueryParameter()}";
//         }
//         Assert.Contains(expectedBackLink, backlink.Href);
//     }
//
//     [Test]
//     [MethodDataSource(nameof(AllCombinationsOf),
//         "change-reason", "check-answers", new[] { PersonStatus.Active, PersonStatus.Deactivated })]
//     [MethodDataSource(nameof(AllCombinationsOf),
//         "check-answers", "change-reason", new[] { PersonStatus.Active, PersonStatus.Deactivated })]
//     public async Task Get_FromCheckAnswers_BacklinkLinksToExpected(string page, string? expectedPage, PersonStatus targetStatus)
//     {
//         var person = await CreatePersonToBecomeStatus(targetStatus);
//
//         var stateBuilder = new SetStatusStateBuilder()
//             .WithInitializedState()
//             .WithUploadEvidenceChoice(false);
//
//         if (targetStatus == PersonStatus.Deactivated)
//         {
//             stateBuilder.WithDeactivateReasonChoice(DeactivateReasonOption.ProblemWithTheRecord);
//         }
//         else
//         {
//             stateBuilder.WithReactivateReasonChoice(ReactivateReasonOption.DeactivatedByMistake);
//         }
//
//         var journeyInstance = await CreateJourneyInstanceAsync(
//             person.PersonId,
//             stateBuilder.Build());
//
//         // Act
//         var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, targetStatus, page, journeyInstance, fromCheckAnswers: true));
//         var response = await HttpClient.SendAsync(request);
//
//         // Assert
//         Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
//         var document = await response.GetDocumentAsync();
//         var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
//         Assert.NotNull(backlink);
//         var expectedBackLink = $"/persons/{person.PersonId}";
//         if (expectedPage is not null)
//         {
//             expectedBackLink += $"/set-status/{targetStatus}/{expectedPage}?{journeyInstance?.GetUniqueIdQueryParameter()}";
//         }
//         Assert.Contains(expectedBackLink, backlink.Href);
//     }
//
//     [Test]
//     [Arguments("change-reason", PersonStatus.Active, "Continue", "Cancel and return to record")]
//     [Arguments("change-reason", PersonStatus.Deactivated, "Continue", "Cancel and return to record")]
//     [Arguments("check-answers", PersonStatus.Active, "Confirm and reactivate record", "Cancel")]
//     [Arguments("check-answers", PersonStatus.Deactivated, "Confirm and deactivate record", "Cancel")]
//     public async Task Get_ContinueAndCancelButtons_ExistOnPage(string page, PersonStatus targetStatus, string continueButtonText, string cancelButtonText)
//     {
//         // Arrange
//         var person = await CreatePersonToBecomeStatus(targetStatus);
//
//         var stateBuilder = new SetStatusStateBuilder()
//             .WithInitializedState()
//             .WithUploadEvidenceChoice(false);
//
//         if (targetStatus == PersonStatus.Deactivated)
//         {
//             stateBuilder.WithDeactivateReasonChoice(DeactivateReasonOption.ProblemWithTheRecord);
//         }
//         else
//         {
//             stateBuilder.WithReactivateReasonChoice(ReactivateReasonOption.DeactivatedByMistake);
//         }
//
//         var journeyInstance = await CreateJourneyInstanceAsync(
//             person.PersonId,
//             stateBuilder.Build());
//
//         var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, targetStatus, page, journeyInstance));
//
//         // Act
//         var response = await HttpClient.SendAsync(request);
//
//         // Assert
//         var doc = await AssertEx.HtmlResponseAsync(response);
//         var form = doc.GetElementByTestId("submit-form") as IHtmlFormElement;
//         Assert.NotNull(form);
//         var buttons = form.GetElementsByTagName("button").OfType<IHtmlButtonElement>();
//         Assert.Collection(buttons,
//             b => Assert.Equal(continueButtonText, b.TrimmedText()),
//             b => Assert.Equal(cancelButtonText, b.TrimmedText()));
//     }
//
//     [Test]
//     [MatrixDataSource]
//     public async Task Post_Cancel_DeletesJourneyAndRedirectsToPersonDetailPage(
//         [Matrix("change-reason", "check-answers")] string page,
//         [Matrix(PersonStatus.Deactivated, PersonStatus.Active)] PersonStatus targetStatus)
//     {
//         // Arrange
//         var person = await CreatePersonToBecomeStatus(targetStatus);
//
//         var stateBuilder = new SetStatusStateBuilder()
//             .WithInitializedState()
//             .WithUploadEvidenceChoice(false);
//
//         if (targetStatus == PersonStatus.Deactivated)
//         {
//             stateBuilder.WithDeactivateReasonChoice(DeactivateReasonOption.ProblemWithTheRecord);
//         }
//         else
//         {
//             stateBuilder.WithReactivateReasonChoice(ReactivateReasonOption.DeactivatedByMistake);
//         }
//
//         var journeyInstance = await CreateJourneyInstanceAsync(
//             person.PersonId,
//             stateBuilder.Build());
//
//         var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, targetStatus, page, journeyInstance));
//
//         // Act
//         var response = await HttpClient.SendAsync(request);
//         var doc = await AssertEx.HtmlResponseAsync(response);
//         var cancelButton = doc.GetElementByTestId("cancel-button") as IHtmlButtonElement;
//         var redirectRequest = new HttpRequestMessage(HttpMethod.Post, cancelButton!.FormAction);
//         var redirectResponse = await HttpClient.SendAsync(redirectRequest);
//
//         // Assert
//         AssertEx.ResponseIsRedirectTo(redirectResponse, $"/persons/{person.PersonId}");
//         journeyInstance = await ReloadJourneyInstance(journeyInstance);
//         Assert.Null(journeyInstance);
//     }
//
//     [Test]
//     [MethodDataSource(nameof(AllCombinationsOf),
//         "change-reason", "check-answers", new[] { PersonStatus.Deactivated, PersonStatus.Active })]
//     [MethodDataSource(nameof(AllCombinationsOf),
//         "check-answers", null, new[] { PersonStatus.Deactivated, PersonStatus.Active })]
//     public async Task Post_RedirectsToExpected(string page, string? expectedPage, PersonStatus targetStatus)
//     {
//         // Arrange
//         var person = await CreatePersonToBecomeStatus(targetStatus);
//
//         var stateBuilder = new SetStatusStateBuilder()
//             .WithInitializedState()
//             .WithUploadEvidenceChoice(false)
//             .WithDeactivateReasonChoice(DeactivateReasonOption.ProblemWithTheRecord)
//             .WithReactivateReasonChoice(ReactivateReasonOption.DeactivatedByMistake);
//
//         var journeyInstance = await CreateJourneyInstanceAsync(
//             person.PersonId,
//             stateBuilder.Build());
//
//         // Act
//         var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, targetStatus, page, journeyInstance))
//         {
//             Content = new SetStatusPostRequestContentBuilder()
//                 .WithDeactivateReason(DeactivateReasonOption.ProblemWithTheRecord)
//                 .WithReactivateReason(ReactivateReasonOption.DeactivatedByMistake)
//                 .WithEvidence(false)
//                 .BuildFormUrlEncoded()
//         };
//
//         // Act
//         var response = await HttpClient.SendAsync(request);
//
//         // Assert
//         var expectedRedirect = $"/persons/{person.PersonId}";
//         if (expectedPage is not null)
//         {
//             expectedRedirect += $"/set-status/{targetStatus}/{expectedPage}?{journeyInstance?.GetUniqueIdQueryParameter()}";
//         }
//
//         AssertEx.ResponseIsRedirectTo(response, expectedRedirect);
//     }
//
//     [Test]
//     [MethodDataSource(nameof(AllCombinationsOf),
//         "change-reason", "check-answers", new[] { PersonStatus.Deactivated, PersonStatus.Active })]
//     [MethodDataSource(nameof(AllCombinationsOf),
//         "check-answers", null, new[] { PersonStatus.Deactivated, PersonStatus.Active })]
//     public async Task Post_FromCheckAnswers_RedirectsToExpected(
//         string page,
//         string? expectedPage,
//         PersonStatus targetStatus)
//     {
//         // Arrange
//         var person = await CreatePersonToBecomeStatus(targetStatus);
//
//         var stateBuilder = new SetStatusStateBuilder()
//             .WithInitializedState()
//             .WithUploadEvidenceChoice(false)
//             .WithDeactivateReasonChoice(DeactivateReasonOption.ProblemWithTheRecord)
//             .WithReactivateReasonChoice(ReactivateReasonOption.DeactivatedByMistake);
//
//         var journeyInstance = await CreateJourneyInstanceAsync(
//             person.PersonId,
//             stateBuilder.Build());
//
//         // Act
//         var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, targetStatus, page, journeyInstance, fromCheckAnswers: true))
//         {
//             Content = new SetStatusPostRequestContentBuilder()
//                 .WithDeactivateReason(DeactivateReasonOption.ProblemWithTheRecord)
//                 .WithReactivateReason(ReactivateReasonOption.DeactivatedByMistake)
//                 .WithEvidence(false)
//                 .BuildFormUrlEncoded()
//         };
//
//         // Act
//         var response = await HttpClient.SendAsync(request);
//
//         // Assert
//         var expectedRedirect = $"/persons/{person.PersonId}";
//         if (expectedPage is not null)
//         {
//             expectedRedirect += $"/set-status/{targetStatus}/{expectedPage}?{journeyInstance?.GetUniqueIdQueryParameter()}";
//         }
//
//         AssertEx.ResponseIsRedirectTo(response, expectedRedirect);
//     }
//
//     private string GetRequestPath(TestData.CreatePersonResult person, PersonStatus targetStatus, string page, JourneyInstance<SetStatusState>? journeyInstance = null, bool? fromCheckAnswers = null) =>
//         $"/persons/{person.PersonId}/set-status/{targetStatus}/{page}?{journeyInstance?.GetUniqueIdQueryParameter()}{(fromCheckAnswers is bool f ? $"&fromCheckAnswers={f}" : "")}";
//
//     private Task<JourneyInstance<SetStatusState>> CreateJourneyInstanceAsync(Guid personId, SetStatusState? state = null) =>
//         CreateJourneyInstance(
//             JourneyNames.SetStatus,
//             state ?? new SetStatusState(),
//             new KeyValuePair<string, object>("personId", personId));
// }
