using NoteCreatedEvent = TeachingRecordSystem.Core.Events.NoteCreatedEvent;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class AddNoteTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Post_ContentIsEmpty_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/notes/add")
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
    public async Task Post_ContentWithoutFile_CreatesNoteAndEventAndRedirects()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var content = Faker.Lorem.Paragraph();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/notes/add")
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
        Assert.Equal($"/persons/{person.PersonId}/notes", response.Headers.Location?.OriginalString);

        var note = await WithDbContextAsync(dbContext => dbContext.Notes.SingleOrDefaultAsync(n => n.PersonId == person.PersonId));
        Assert.NotNull(note);
        Assert.Equal(content, note.Content);
        Assert.Null(note.FileId);

        Events.AssertProcessesCreated(x =>
        {
            Assert.Equal(ProcessType.NoteCreating, x.ProcessContext.ProcessType);
            Assert.Collection(x.Events, e => Assert.IsType<NoteCreatedEvent>(e));
        });
    }

    [Fact]
    public async Task Post_ContentWithFile_CreatesNoteAndEventAndRedirects()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var content = Faker.Lorem.Paragraph();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/notes/add")
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "Content", content },
                { "File", (CreateEvidenceFileBinaryContent(), "Attachment.jpeg") }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}/notes", response.Headers.Location?.OriginalString);

        var note = await WithDbContextAsync(dbContext => dbContext.Notes.SingleOrDefaultAsync(n => n.PersonId == person.PersonId));
        Assert.NotNull(note);
        Assert.Equal(content, note.Content);
        Assert.NotNull(note.FileId);

        Events.AssertProcessesCreated(x =>
        {
            Assert.Equal(ProcessType.NoteCreating, x.ProcessContext.ProcessType);
            Assert.Collection(x.Events, e => Assert.IsType<NoteCreatedEvent>(e));
        });
    }

    [Theory]
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(httpMethod, $"/persons/{person.PersonId}/notes/add");
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
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }
}
