namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class TrnRequestProcessesTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    // A TRN request that creates a person is how most records come into TRS, so the creation has to render on
    // the person's change history from each of the processes that can do it.
    [Theory]
    [InlineData(ProcessType.TrnRequestCreating, "TRN request received", "Record created from a TRN request.")]
    [InlineData(ProcessType.TrnRequestResolving, "TRN request resolved", "Record created while resolving a TRN request.")]
    [InlineData(ProcessType.TrnRequestActivating, "TRN request activated", "Record created when a dormant TRN request was activated.")]
    public async Task ProcessThatCreatedAPerson_RendersTheRecordDetails(
        ProcessType processType,
        string expectedTitle,
        string expectedSummary)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber().WithEmailAddress().WithGender());
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var user = await TestData.CreateUserAsync();

        var events = new List<IEvent>();

        if (processType == ProcessType.TrnRequestCreating)
        {
            events.Add(CreateTrnRequestCreatedEvent(applicationUser.UserId, resolvedPersonId: person.PersonId));
        }

        events.Add(new PersonCreatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            Details = EventModels.PersonDetails.FromModel(person),
            TrnRequestMetadata = null
        });

        var process = await TestData.CreateProcessAsync(
            processType,
            user.UserId,
            changeReason: null,
            events.ToArray());

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, personId: processType == ProcessType.TrnRequestCreating ? person.PersonId : null);

        // Assert
        AssertTitle(entry, expectedTitle);
        Assert.Equal(expectedSummary, entry.GetElementByTestId("trn-request-summary")?.TrimmedText());

        entry.AssertSummaryListRowValue("details", "Name", v =>
            Assert.Equal($"{person.FirstName} {person.MiddleName} {person.LastName}", v.TrimmedText()));
        entry.AssertSummaryListRowValue("details", "Date of birth", v =>
            Assert.Equal(person.DateOfBirth!.Value.ToString(WebConstants.DateDisplayFormat), v.TrimmedText()));
    }

    [Fact]
    public async Task TrnRequestCreatingProcessWithSupportTaskAndNoResolvedPerson_RendersWithoutRecordDetails()
    {
        // Arrange
        var user = await TestData.CreateUserAsync();
        var (supportTask, requestData, _) = await TestData.CreateTrnRequestSupportTaskAsync();

        var process = await TestData.CreateProcessAsync(
            ProcessType.TrnRequestCreating,
            user.UserId,
            changeReason: null,
            CreateTrnRequestCreatedEvent(requestData.ApplicationUserId, resolvedPersonId: null),
            new SupportTaskCreatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTask = EventModels.SupportTask.FromModel(supportTask)
            });

        // Act
        var entry = await GetEntryHtmlAsync(
            process.ProcessId,
            contextType: "supportTask",
            supportTaskReference: supportTask.SupportTaskReference);

        // Assert
        AssertTitle(entry, "TRN request received");
        Assert.Null(entry.GetElementByTestId("trn-request-summary"));
        Assert.Contains("TRN request support task", entry.GetElementByTestId("support-task-created-message")?.TextContent);
        Assert.Null(entry.GetElementByTestId("created-task-link"));
        Assert.Null(entry.GetElementByTestId("details"));
    }

    [Fact]
    public async Task TrnRequestCreatingProcessInSupportTaskContextWithResolvedPerson_RendersPersonLink()
    {
        // Arrange
        var user = await TestData.CreateUserAsync();
        var supportTask = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync();
        var resolvedPersonId = supportTask.TrnRequestMetadata!.ResolvedPersonId!.Value;

        var process = await TestData.CreateProcessAsync(
            ProcessType.TrnRequestCreating,
            user.UserId,
            changeReason: null,
            CreateTrnRequestCreatedEvent(
                supportTask.TrnRequestMetadata!.ApplicationUserId,
                resolvedPersonId: resolvedPersonId),
            new SupportTaskCreatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTask = EventModels.SupportTask.FromModel(supportTask)
            });

        // Act
        var entry = await GetEntryHtmlAsync(
            process.ProcessId,
            contextType: "supportTask",
            supportTaskReference: supportTask.SupportTaskReference);

        // Assert
        Assert.Contains("TRN request matched to", entry.GetElementByTestId("trn-request-summary")?.TextContent);
        var matchedPersonLink = entry.GetElementByTestId("matched-person-link");
        Assert.NotNull(matchedPersonLink);
        Assert.EndsWith($"/persons/{resolvedPersonId}", matchedPersonLink!.GetAttribute("href"));
    }

    [Theory]
    [InlineData("person", true, false)]
    [InlineData("oneLogin", false, true)]
    [InlineData("supportTask", true, true)]
    public async Task TrnRequestCreatingProcessWithOneLoginUpdatedEvent_LinksRenderBasedOnContext(
        string contextType,
        bool expectOneLoginLink,
        bool expectPersonLink)
    {
        // Arrange
        var user = await TestData.CreateUserAsync();
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);
        var (supportTask, _, _) = await TestData.CreateTrnRequestSupportTaskAsync();

        var process = await TestData.CreateProcessAsync(
            ProcessType.TrnRequestCreating,
            user.UserId,
            changeReason: null,
            CreateTrnRequestCreatedEvent(
                applicationUser.UserId,
                resolvedPersonId: person.PersonId,
                oneLoginUserSubject: oneLoginUser.Subject),
            new SupportTaskCreatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTask = EventModels.SupportTask.FromModel(supportTask)
            },
            new OneLoginUserUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                OneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser),
                OldOneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser) with { PersonId = null },
                Changes = OneLoginUserUpdatedEventChanges.PersonId
            });

        // Act
        var entry = await GetEntryHtmlAsync(
            process.ProcessId,
            personId: contextType == "person" ? person.PersonId : null,
            contextType: contextType == "person" ? null : contextType,
            oneLoginSubject: contextType == "oneLogin" ? oneLoginUser.Subject : null,
            supportTaskReference: contextType == "supportTask" ? supportTask.SupportTaskReference : null);

        // Assert
        var oneLoginLink = entry.GetElementByTestId("one-login-link");
        if (expectOneLoginLink)
        {
            Assert.NotNull(oneLoginLink);
            Assert.Contains(oneLoginUser.EmailAddress!, oneLoginLink!.TextContent);
        }
        else
        {
            Assert.Null(oneLoginLink);
        }

        var personLink = entry.GetElementByTestId("person-link");
        if (expectPersonLink)
        {
            Assert.NotNull(personLink);
            Assert.Contains(person.FirstName, personLink!.TextContent);
            Assert.Contains(person.LastName, personLink.TextContent);
        }
        else
        {
            Assert.Null(personLink);
        }
    }

    [Fact]
    public async Task TrnRequestManualChecksNeededTaskCompletingProcessInPersonContext_RendersExpectedContent()
    {
        // Arrange
        var user = await TestData.CreateUserAsync();
        var supportTask = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync();
        var resolvedPersonId = supportTask.TrnRequestMetadata!.ResolvedPersonId!.Value;

        var process = await TestData.CreateProcessAsync(
            ProcessType.TrnRequestManualChecksNeededTaskCompleting,
            user.UserId,
            changeReason: null,
            CreateResolvedTrnRequestUpdatedEvent(
                supportTask.TrnRequestMetadata.ApplicationUserId,
                resolvedPersonId),
            CreateManualChecksNeededSupportTaskCompletedEvent(supportTask));

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, personId: resolvedPersonId);

        // Assert
        AssertTitle(entry, "TRN request manual checks needed task completed");
        Assert.Contains("Checks completed", entry.GetElementByTestId("outcome-tag")?.TextContent);
        Assert.Equal("TRN request completed and matched to this record.", entry.GetElementByTestId("trn-request-summary")?.TrimmedText());

        var taskClosedMessage = entry.GetElementByTestId("task-closed-message");
        Assert.NotNull(taskClosedMessage);
        Assert.NotNull(entry.GetElementByTestId("support-task-link"));
    }

    [Fact]
    public async Task TrnRequestManualChecksNeededTaskCompletingProcessInSupportTaskContext_RendersExpectedContent()
    {
        // Arrange
        var user = await TestData.CreateUserAsync();
        var supportTask = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync();
        var resolvedPersonId = supportTask.TrnRequestMetadata!.ResolvedPersonId!.Value;

        var process = await TestData.CreateProcessAsync(
            ProcessType.TrnRequestManualChecksNeededTaskCompleting,
            user.UserId,
            changeReason: null,
            CreateResolvedTrnRequestUpdatedEvent(
                supportTask.TrnRequestMetadata.ApplicationUserId,
                resolvedPersonId),
            CreateManualChecksNeededSupportTaskCompletedEvent(supportTask));

        // Act
        var entry = await GetEntryHtmlAsync(
            process.ProcessId,
            contextType: "supportTask",
            supportTaskReference: supportTask.SupportTaskReference);

        // Assert
        AssertTitle(entry, "TRN request manual checks needed task completed");
        Assert.Contains("TRN request completed and matched to", entry.GetElementByTestId("trn-request-summary")?.TextContent);

        var personLink = entry.GetElementByTestId("person-link");
        Assert.NotNull(personLink);
        Assert.EndsWith($"/persons/{resolvedPersonId}", personLink!.GetAttribute("href"));

        Assert.Equal("Task closed.", entry.GetElementByTestId("task-closed-message")?.TrimmedText());
        Assert.Null(entry.GetElementByTestId("support-task-link"));
    }

    [Fact]
    public async Task TrnRequestResettingProcessInSupportTaskContext_RendersExpectedContent()
    {
        // Arrange
        var user = await TestData.CreateUserAsync();
        var (supportTask, requestData, _) = await TestData.CreateTrnRequestSupportTaskAsync();
        var reason = "Reset request to pending for manual checks.";

        var process = await TestData.CreateProcessAsync(
            ProcessType.TrnRequestResetting,
            user.UserId,
            changeReason: null,
            new SupportTaskCreatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTask = EventModels.SupportTask.FromModel(supportTask)
            },
            CreateResetTrnRequestUpdatedEvent(requestData.ApplicationUserId, reason));

        // Act
        var entry = await GetEntryHtmlAsync(
            process.ProcessId,
            contextType: "supportTask",
            supportTaskReference: supportTask.SupportTaskReference);

        // Assert
        AssertTitle(entry, "TRN request reset");
        Assert.Equal("TRN request reset and moved to pending.", entry.GetElementByTestId("trn-request-summary")?.TrimmedText());
        Assert.Contains($"TRN request support task {supportTask.SupportTaskReference} was created.", entry.GetElementByTestId("support-task-created-message")?.TrimmedText());
        Assert.Null(entry.GetElementByTestId("created-task-link"));
        entry.AssertSummaryListRowValue("change-reason", "Reason", v => Assert.Equal(reason, v.TrimmedText()));
    }

    [Fact]
    public async Task TrnRequestResolvingProcessInSupportTaskContextWithResolvedPerson_RendersPersonLinkAndTaskClosedMessage()
    {
        // Arrange
        var user = await TestData.CreateUserAsync();
        var person = await TestData.CreatePersonAsync();
        var (supportTask, requestData, _) = await TestData.CreateTrnRequestSupportTaskAsync();

        var process = await TestData.CreateProcessAsync(
            ProcessType.TrnRequestResolving,
            user.UserId,
            changeReason: null,
            CreateResolvedTrnRequestUpdatedEvent(requestData.ApplicationUserId, person.PersonId),
            CreateResolvedSupportTaskUpdatedEvent(supportTask));

        // Act
        var entry = await GetEntryHtmlAsync(
            process.ProcessId,
            contextType: "supportTask",
            supportTaskReference: supportTask.SupportTaskReference);

        // Assert
        Assert.Contains("TRN request resolved to", entry.GetElementByTestId("trn-request-summary")?.TextContent);

        var matchedPersonLink = entry.GetElementByTestId("matched-person-link");
        Assert.NotNull(matchedPersonLink);
        Assert.EndsWith($"/persons/{person.PersonId}", matchedPersonLink!.GetAttribute("href"));

        var taskClosedMessage = entry.GetElementByTestId("task-closed-message");
        Assert.NotNull(taskClosedMessage);
        Assert.Contains("Task closed.", taskClosedMessage!.TrimmedText());
        Assert.Null(entry.GetElementByTestId("support-task-link"));
    }

    [Fact]
    public async Task TrnRequestResolvingProcessWithComments_RendersChangeReason()
    {
        // Arrange
        var user = await TestData.CreateUserAsync();
        var person = await TestData.CreatePersonAsync();
        var (supportTask, requestData, _) = await TestData.CreateTrnRequestSupportTaskAsync();
        var comments = TestData.GenerateLoremIpsum();

        var process = await TestData.CreateProcessAsync(
            ProcessType.TrnRequestResolving,
            user.UserId,
            changeReason: null,
            CreateResolvedTrnRequestUpdatedEvent(requestData.ApplicationUserId, person.PersonId),
            CreateResolvedSupportTaskUpdatedEvent(supportTask, comments));

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, person.PersonId);

        // Assert
        entry.AssertSummaryListRowValue("change-reason", "Reason", v => Assert.Equal(comments, v.TrimmedText()));
    }

    [Theory]
    [InlineData("person", true, false)]
    [InlineData("oneLogin", false, true)]
    [InlineData("supportTask", true, true)]
    public async Task TrnRequestResolvingProcessWithOneLoginUpdatedEvent_LinksRenderBasedOnContext(
        string contextType,
        bool expectOneLoginLink,
        bool expectPersonLink)
    {
        // Arrange
        var user = await TestData.CreateUserAsync();
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);
        var (supportTask, _, _) = await TestData.CreateTrnRequestSupportTaskAsync();

        var process = await TestData.CreateProcessAsync(
            ProcessType.TrnRequestResolving,
            user.UserId,
            changeReason: null,
            CreateResolvedTrnRequestUpdatedEvent(
                applicationUser.UserId,
                resolvedPersonId: person.PersonId,
                oneLoginUserSubject: oneLoginUser.Subject),
            CreateResolvedSupportTaskUpdatedEvent(supportTask),
            new OneLoginUserUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                OneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser),
                OldOneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser) with { PersonId = null },
                Changes = OneLoginUserUpdatedEventChanges.PersonId
            });

        // Act
        var entry = await GetEntryHtmlAsync(
            process.ProcessId,
            personId: contextType == "person" ? person.PersonId : null,
            contextType: contextType == "person" ? null : contextType,
            oneLoginSubject: contextType == "oneLogin" ? oneLoginUser.Subject : null,
            supportTaskReference: contextType == "supportTask" ? supportTask.SupportTaskReference : null);

        // Assert
        var oneLoginLink = entry.GetElementByTestId("one-login-link");
        if (expectOneLoginLink)
        {
            Assert.NotNull(oneLoginLink);
            Assert.Contains(oneLoginUser.EmailAddress!, oneLoginLink!.TextContent);
        }
        else
        {
            Assert.Null(oneLoginLink);
        }

        var personLink = entry.GetElementByTestId("person-link");
        if (expectPersonLink)
        {
            Assert.NotNull(personLink);
            Assert.Contains(person.FirstName, personLink!.TextContent);
            Assert.Contains(person.LastName, personLink.TextContent);
        }
        else
        {
            Assert.Null(personLink);
        }
    }

    [Fact]
    public async Task TrnRequestActivatingProcessWithSupportTaskAndNoResolvedPerson_RendersWithoutRecordDetails()
    {
        // Arrange
        var user = await TestData.CreateUserAsync();
        var (supportTask, requestData, _) = await TestData.CreateTrnRequestSupportTaskAsync();

        var process = await TestData.CreateProcessAsync(
            ProcessType.TrnRequestActivating,
            user.UserId,
            changeReason: null,
            CreateTrnRequestUpdatedEvent(requestData.ApplicationUserId, resolvedPersonId: null),
            new SupportTaskCreatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTask = EventModels.SupportTask.FromModel(supportTask)
            });

        // Act
        var entry = await GetEntryHtmlAsync(
            process.ProcessId,
            contextType: "supportTask",
            supportTaskReference: supportTask.SupportTaskReference);

        // Assert
        AssertTitle(entry, "TRN request activated");
        Assert.Null(entry.GetElementByTestId("trn-request-summary"));
        Assert.Contains("TRN request support task", entry.GetElementByTestId("support-task-created-message")?.TextContent);
        Assert.Null(entry.GetElementByTestId("created-task-link"));
        Assert.Null(entry.GetElementByTestId("details"));
    }

    [Fact]
    public async Task TrnRequestActivatingProcessInSupportTaskContextWithResolvedPerson_RendersPersonLink()
    {
        // Arrange
        var user = await TestData.CreateUserAsync();
        var supportTask = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync();
        var resolvedPersonId = supportTask.TrnRequestMetadata!.ResolvedPersonId!.Value;

        var process = await TestData.CreateProcessAsync(
            ProcessType.TrnRequestActivating,
            user.UserId,
            changeReason: null,
            CreateTrnRequestUpdatedEvent(
                supportTask.TrnRequestMetadata!.ApplicationUserId,
                resolvedPersonId: resolvedPersonId),
            new SupportTaskCreatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTask = EventModels.SupportTask.FromModel(supportTask)
            });

        // Act
        var entry = await GetEntryHtmlAsync(
            process.ProcessId,
            contextType: "supportTask",
            supportTaskReference: supportTask.SupportTaskReference);

        // Assert
        Assert.Contains("activated and matched to", entry.GetElementByTestId("trn-request-summary")?.TextContent);
        var matchedPersonLink = entry.GetElementByTestId("matched-person-link");
        Assert.NotNull(matchedPersonLink);
        Assert.EndsWith($"/persons/{resolvedPersonId}", matchedPersonLink!.GetAttribute("href"));
    }

    [Theory]
    [InlineData("person", true, false)]
    [InlineData("oneLogin", false, true)]
    [InlineData("supportTask", true, true)]
    public async Task TrnRequestActivatingProcessWithOneLoginUpdatedEvent_LinksRenderBasedOnContext(
        string contextType,
        bool expectOneLoginLink,
        bool expectPersonLink)
    {
        // Arrange
        var user = await TestData.CreateUserAsync();
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);
        var (supportTask, _, _) = await TestData.CreateTrnRequestSupportTaskAsync();

        var process = await TestData.CreateProcessAsync(
            ProcessType.TrnRequestActivating,
            user.UserId,
            changeReason: null,
            CreateTrnRequestUpdatedEvent(
                applicationUser.UserId,
                resolvedPersonId: person.PersonId,
                oneLoginUserSubject: oneLoginUser.Subject),
            new SupportTaskCreatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTask = EventModels.SupportTask.FromModel(supportTask)
            },
            new OneLoginUserUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                OneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser),
                OldOneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser) with { PersonId = null },
                Changes = OneLoginUserUpdatedEventChanges.PersonId
            });

        // Act
        var entry = await GetEntryHtmlAsync(
            process.ProcessId,
            personId: contextType == "person" ? person.PersonId : null,
            contextType: contextType == "person" ? null : contextType,
            oneLoginSubject: contextType == "oneLogin" ? oneLoginUser.Subject : null,
            supportTaskReference: contextType == "supportTask" ? supportTask.SupportTaskReference : null);

        // Assert
        var oneLoginLink = entry.GetElementByTestId("one-login-link");
        if (expectOneLoginLink)
        {
            Assert.NotNull(oneLoginLink);
            Assert.Contains(oneLoginUser.EmailAddress!, oneLoginLink!.TextContent);
        }
        else
        {
            Assert.Null(oneLoginLink);
        }

        var personLink = entry.GetElementByTestId("person-link");
        if (expectPersonLink)
        {
            Assert.NotNull(personLink);
            Assert.Contains(person.FirstName, personLink!.TextContent);
            Assert.Contains(person.LastName, personLink.TextContent);
        }
        else
        {
            Assert.Null(personLink);
        }
    }

    private TrnRequestCreatedEvent CreateTrnRequestCreatedEvent(
        Guid applicationUserId,
        Guid? resolvedPersonId,
        string? oneLoginUserSubject = null)
    {
        return new TrnRequestCreatedEvent
        {
            EventId = Guid.NewGuid(),
            TrnRequest = new EventModels.TrnRequestMetadata
            {
                ApplicationUserId = applicationUserId,
                RequestId = "TEST-TRN-1",
                CreatedOn = TimeProvider.UtcNow,
                IdentityVerified = oneLoginUserSubject is not null,
                EmailAddress = TestData.GenerateUniqueEmail(),
                OneLoginUserSubject = oneLoginUserSubject,
                FirstName = "Megan",
                MiddleName = "Thee",
                LastName = "Stallion",
                PreviousFirstName = null,
                PreviousLastName = null,
                Name = ["Megan", "Thee", "Stallion"],
                DateOfBirth = new DateOnly(1990, 1, 1),
                PotentialDuplicate = null,
                NationalInsuranceNumber = null,
                Gender = null,
                AddressLine1 = null,
                AddressLine2 = null,
                AddressLine3 = null,
                City = null,
                Postcode = null,
                Country = null,
                TrnToken = null,
                ResolvedPersonId = resolvedPersonId,
                Matches = null,
                NpqApplicationId = null,
                NpqEvidenceFileId = null,
                NpqEvidenceFileName = null,
                NpqName = null,
                NpqTrainingProvider = null,
                NpqWorkingInEducationalSetting = null,
                Status = resolvedPersonId is not null ? TrnRequestStatus.Completed : TrnRequestStatus.Pending
            }
        };
    }

    private TrnRequestUpdatedEvent CreateTrnRequestUpdatedEvent(
        Guid applicationUserId,
        Guid? resolvedPersonId,
        string? oneLoginUserSubject = null)
    {
        var trnRequest = CreateTrnRequestCreatedEvent(applicationUserId, resolvedPersonId, oneLoginUserSubject).TrnRequest;

        return new TrnRequestUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            SourceApplicationUserId = applicationUserId,
            RequestId = trnRequest.RequestId,
            Changes = TrnRequestUpdatedChanges.Status |
                (resolvedPersonId is not null ? TrnRequestUpdatedChanges.ResolvedPersonId : TrnRequestUpdatedChanges.None),
            TrnRequest = trnRequest,
            OldTrnRequest = trnRequest with
            {
                Status = TrnRequestStatus.Dormant,
                ResolvedPersonId = null
            },
            ReasonDetails = null
        };
    }

    private TrnRequestUpdatedEvent CreateResetTrnRequestUpdatedEvent(Guid applicationUserId, string reason)
    {
        var oldTrnRequest = CreateTrnRequestCreatedEvent(applicationUserId, resolvedPersonId: Guid.NewGuid()).TrnRequest with
        {
            Status = TrnRequestStatus.Completed
        };

        return new TrnRequestUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            SourceApplicationUserId = applicationUserId,
            RequestId = oldTrnRequest.RequestId,
            Changes = TrnRequestUpdatedChanges.Status | TrnRequestUpdatedChanges.ResolvedPersonId,
            OldTrnRequest = oldTrnRequest,
            TrnRequest = oldTrnRequest with
            {
                Status = TrnRequestStatus.Pending,
                ResolvedPersonId = null
            },
            ReasonDetails = reason
        };
    }

    private TrnRequestUpdatedEvent CreateResolvedTrnRequestUpdatedEvent(
        Guid applicationUserId,
        Guid resolvedPersonId,
        string? oneLoginUserSubject = null)
    {
        var trnRequest = CreateTrnRequestCreatedEvent(applicationUserId, resolvedPersonId, oneLoginUserSubject).TrnRequest;

        return new TrnRequestUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            SourceApplicationUserId = applicationUserId,
            RequestId = trnRequest.RequestId,
            Changes = TrnRequestUpdatedChanges.Status | TrnRequestUpdatedChanges.ResolvedPersonId,
            TrnRequest = trnRequest with
            {
                Status = TrnRequestStatus.Completed,
                ResolvedPersonId = resolvedPersonId
            },
            OldTrnRequest = trnRequest with
            {
                Status = TrnRequestStatus.Pending,
                ResolvedPersonId = null
            },
            ReasonDetails = null
        };
    }

    private SupportTaskUpdatedEvent CreateResolvedSupportTaskUpdatedEvent(Core.DataStore.Postgres.Models.SupportTask supportTask, string? comments = null)
    {
        var oldSupportTask = EventModels.SupportTask.FromModel(supportTask);

        return new SupportTaskUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            SupportTaskReference = supportTask.SupportTaskReference,
            Changes = SupportTaskUpdatedEventChanges.Status | SupportTaskUpdatedEventChanges.Outcome,
            SupportTask = oldSupportTask with
            {
                Status = SupportTaskStatus.Closed,
                Outcome = SupportTaskOutcome.TrnRequest_ResolvedWithExistingPerson
            },
            OldSupportTask = oldSupportTask,
            Comments = comments,
            RejectionReason = null
        };
    }

    private SupportTaskUpdatedEvent CreateManualChecksNeededSupportTaskCompletedEvent(Core.DataStore.Postgres.Models.SupportTask supportTask)
    {
        var oldSupportTask = EventModels.SupportTask.FromModel(supportTask);

        return new SupportTaskUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            SupportTaskReference = supportTask.SupportTaskReference,
            Changes = SupportTaskUpdatedEventChanges.Status | SupportTaskUpdatedEventChanges.Outcome,
            SupportTask = oldSupportTask with
            {
                Status = SupportTaskStatus.Closed,
                Outcome = SupportTaskOutcome.TrnRequestManualChecksNeeded_Completed
            },
            OldSupportTask = oldSupportTask,
            Comments = null,
            RejectionReason = null
        };
    }
}
