namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.SupportTaskDetail;

public class AddNoteTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_ValidSupportTask_DisplaysForm()
    {
        // Arrange
        var supportTask = await TestData.CreateTrnRequestSupportTaskAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/{supportTask.SupportTask.SupportTaskReference}/notes/add");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_ContentIsEmpty_ReturnsError()
    {
        // Arrange
        var supportTask = await TestData.CreateTrnRequestSupportTaskAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/{supportTask.SupportTask.SupportTaskReference}/notes/add")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Content", "" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Content", "Enter text for the note");
    }

    [Fact]
    public async Task Post_ContentTooLong_ReturnsError()
    {
        // Arrange
        var supportTask = await TestData.CreateTrnRequestSupportTaskAsync();

        var content = new string('a', 4001);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/{supportTask.SupportTask.SupportTaskReference}/notes/add")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Content", content }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Content", "Note must be 4000 characters or less");
    }

    [Fact]
    public async Task Post_ValidContent_CreatesNoteAndRedirects()
    {
        // Arrange
        var supportTask = await TestData.CreateTrnRequestSupportTaskAsync();

        var content = Faker.Lorem.Paragraph();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/{supportTask.SupportTask.SupportTaskReference}/notes/add")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Content", content }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var note = await WithDbContextAsync(dbContext =>
            dbContext.SupportTaskNotes.SingleOrDefaultAsync(n => n.SupportTaskReference == supportTask.SupportTask.SupportTaskReference));
        Assert.NotNull(note);
        Assert.Equal(content, note.Content);
        Assert.Equal(supportTask.SupportTask.SupportTaskReference, note.SupportTaskReference);

        Events.AssertProcessesCreated(x =>
        {
            Assert.Equal(ProcessType.SupportTaskNoteCreating, x.ProcessContext.ProcessType);
            Assert.Collection(x.Events, e =>
            {
                var noteCreatedEvent = Assert.IsType<SupportTaskNoteCreatedEvent>(e);
                Assert.Equal(note.SupportTaskNoteId, noteCreatedEvent.SupportTaskNote.SupportTaskNoteId);
                Assert.Equal(supportTask.SupportTask.SupportTaskReference, noteCreatedEvent.SupportTaskNote.SupportTaskReference);
                Assert.Equal(content, noteCreatedEvent.SupportTaskNote.Content);
            });
        });
    }

    [Fact]
    public async Task Post_ValidContent_RedirectsToDetailWithNotesExpandedAndShowsFlashMessage()
    {
        // Arrange
        var supportTask = await TestData.CreateTrnRequestSupportTaskAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/{supportTask.SupportTask.SupportTaskReference}/notes/add")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Content", Faker.Lorem.Paragraph() }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var location = response.Headers.Location?.OriginalString;
        Assert.NotNull(location);
        Assert.StartsWith($"/support-tasks/{supportTask.SupportTask.SupportTaskReference}", location);
        Assert.Contains("xpandNotes=True", location);

        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashNotificationBanner(nextPageDoc, "Note added");
    }

    [Fact]
    public async Task Post_MaxLengthContent_CreatesNoteSuccessfully()
    {
        // Arrange
        var supportTask = await TestData.CreateTrnRequestSupportTaskAsync();

        var content = new string('a', 4000);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/{supportTask.SupportTask.SupportTaskReference}/notes/add")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Content", content }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var note = await WithDbContextAsync(dbContext =>
            dbContext.SupportTaskNotes.SingleOrDefaultAsync(n => n.SupportTaskReference == supportTask.SupportTask.SupportTaskReference));
        Assert.NotNull(note);
        Assert.Equal(content, note.Content);
    }

    [Theory]
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task SupportTaskIsClosed_ReturnsNotFound(HttpMethod httpMethod)
    {
        // Arrange
        var supportTask = await TestData.CreateTrnRequestSupportTaskAsync(configure: t => t.WithStatus(SupportTaskStatus.Closed));

        var request = new HttpRequestMessage(httpMethod, $"/support-tasks/{supportTask.SupportTask.SupportTaskReference}/notes/add");
        if (httpMethod == HttpMethod.Post)
        {
            request.Content = new FormUrlEncodedContentBuilder
            {
                { "Content", Faker.Lorem.Paragraph() }
            };
        }

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_MultipleTasks_CreatesNoteForCorrectTask()
    {
        // Arrange
        var supportTask1 = await TestData.CreateTrnRequestSupportTaskAsync();
        var supportTask2 = await TestData.CreateTrnRequestSupportTaskAsync();

        var content = Faker.Lorem.Paragraph();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/{supportTask1.SupportTask.SupportTaskReference}/notes/add")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Content", content }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        await WithDbContextAsync(async dbContext =>
        {
            var note1 = await dbContext.SupportTaskNotes
                .SingleOrDefaultAsync(n => n.SupportTaskReference == supportTask1.SupportTask.SupportTaskReference);
            Assert.NotNull(note1);
            Assert.Equal(content, note1.Content);

            var note2 = await dbContext.SupportTaskNotes
                .SingleOrDefaultAsync(n => n.SupportTaskReference == supportTask2.SupportTask.SupportTaskReference);
            Assert.Null(note2);
        });
    }
}
