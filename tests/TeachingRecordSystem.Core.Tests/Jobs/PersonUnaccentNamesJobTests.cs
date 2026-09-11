using TeachingRecordSystem.Core.Jobs;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class PersonUnaccentNamesJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    [Theory]
    [InlineData("José", "Andre", "Muller", "Jose", "Andre", "Muller")]
    [InlineData("Jose", "André", "Muller", "Jose", "Andre", "Muller")]
    [InlineData("Jose", "Andre", "Müller", "Jose", "Andre", "Muller")]
    [InlineData("José", "André", "Müller", "Jose", "Andre", "Muller")]
    [InlineData("José", "", "Müller", "Jose", "", "Muller")]
    [InlineData("Jose", "", "Muller", "Jose", "", "Muller")]
    public async Task ExecuteAsync_UnaccentsEachName(
        string firstName, string middleName, string lastName,
        string expectedFirstName, string expectedMiddleName, string expectedLastName)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName(firstName)
            .WithMiddleName(middleName)
            .WithLastName(lastName));

        // Act
        await WithServiceAsync<PersonUnaccentNamesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var updatedPerson = await dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId);
            Assert.Equal(expectedFirstName, updatedPerson.FirstName);
            Assert.Equal(expectedMiddleName, updatedPerson.MiddleName);
            Assert.Equal(expectedLastName, updatedPerson.LastName);
        });
    }

    [Fact]
    public async Task ExecuteAsync_MultipleBatches_UpdatesAllPersonsAndCanRunAgain()
    {
        // Arrange
        var personIds = new List<Guid>();
        for (var i = 0; i < 201; i++)
        {
            var person = await TestData.CreatePersonAsync(p => p
                .WithFirstName("José")
                .WithMiddleName("André")
                .WithLastName("Müller"));
            personIds.Add(person.PersonId);
        }

        // Act
        await WithServiceAsync<PersonUnaccentNamesJob>(job => job.ExecuteAsync(CancellationToken.None));
        await WithServiceAsync<PersonUnaccentNamesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var persons = await dbContext.Persons.Where(p => personIds.Contains(p.PersonId)).ToArrayAsync();
            Assert.Equal(201, persons.Length);
            Assert.All(persons, p =>
            {
                Assert.Equal("Jose", p.FirstName);
                Assert.Equal("Andre", p.MiddleName);
                Assert.Equal("Muller", p.LastName);
            });
        });
    }
}
