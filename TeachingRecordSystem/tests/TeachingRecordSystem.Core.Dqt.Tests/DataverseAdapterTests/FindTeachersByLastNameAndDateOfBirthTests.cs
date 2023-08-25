
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;

namespace TeachingRecordSystem.Core.Dqt.Tests.DataverseAdapterTests;
public class FindTeachersByLastNameAndDateOfBirthTests
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly DataverseAdapter _dataverseAdapter;
    private readonly IOrganizationServiceAsync _organizationService;

    public FindTeachersByLastNameAndDateOfBirthTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _dataverseAdapter = _dataScope.CreateDataverseAdapter();
        _organizationService = _dataScope.OrganizationService;
    }

    [Fact]
    public async Task Given_match_on_lastname_and_not_previouslastname_return_correct_records()
    {
        // Arrange
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var previousLastName = Faker.Name.Last();
        var qtsDate = new DateOnly(1985, 01, 01);
        var dateOfBirth = new DateOnly(1978, 4, 15);


        var teacher1Id = await _organizationService.CreateAsync(new Contact()
        {
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            BirthDate = dateOfBirth.ToDateTime(),
            dfeta_QTSDate = qtsDate.ToDateTime(),
            dfeta_PreviousLastName = previousLastName
        });

        await _organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new Contact()
            {
                Id = teacher1Id,
                dfeta_TRNAllocateRequest = DateTime.UtcNow
            }
        });

        // Act
        var teachers = await _dataverseAdapter.FindTeachersByLastNameAndDateOfBirth(lastName, dateOfBirth, columnNames: new[]
            {
                Contact.Fields.dfeta_TRN,
                Contact.Fields.BirthDate,
                Contact.Fields.FirstName,
                Contact.Fields.MiddleName,
                Contact.Fields.LastName,
                Contact.Fields.dfeta_StatedFirstName,
                Contact.Fields.dfeta_StatedMiddleName,
                Contact.Fields.dfeta_StatedLastName,
                Contact.Fields.dfeta_TRNAllocateRequest,
                Contact.Fields.dfeta_PreviousLastName
            });

        // Assert
        Assert.Collection(teachers,
            teacher1 =>
            {
                Assert.Equal(teacher1Id, teacher1.Id);
                Assert.Equal(lastName, teacher1.LastName);
                Assert.Equal(previousLastName, teacher1.dfeta_PreviousLastName);
            }
        );
    }

    [Fact]
    public async Task Given_match_on_previouslastname_and_lastname_return_correct_records()
    {
        // Arrange
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var previousLastName = Faker.Name.Last();
        var qtsDate = new DateOnly(1985, 01, 01);
        var dateOfBirth = new DateOnly(1978, 4, 15);

        var teacher1Id = await _organizationService.CreateAsync(new Contact()
        {
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            BirthDate = dateOfBirth.ToDateTime(),
            dfeta_QTSDate = qtsDate.ToDateTime(),
        });

        var teacher2Id = await _organizationService.CreateAsync(new Contact()
        {
            FirstName = firstName,
            MiddleName = middleName,
            LastName = Faker.Name.Last(),
            BirthDate = dateOfBirth.ToDateTime(),
            dfeta_QTSDate = qtsDate.ToDateTime(),
            dfeta_PreviousLastName = lastName
        });

        await _organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new Contact()
            {
                Id = teacher1Id,
                dfeta_TRNAllocateRequest = DateTime.UtcNow
            }
        });
        await _organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new Contact()
            {
                Id = teacher2Id,
                dfeta_TRNAllocateRequest = DateTime.UtcNow.AddDays(1)
            }
        });

        // Act
        var teachers = await _dataverseAdapter.FindTeachersByLastNameAndDateOfBirth(lastName, dateOfBirth, columnNames: new[]
            {
                Contact.Fields.dfeta_TRN,
                Contact.Fields.BirthDate,
                Contact.Fields.FirstName,
                Contact.Fields.MiddleName,
                Contact.Fields.LastName,
                Contact.Fields.dfeta_StatedFirstName,
                Contact.Fields.dfeta_StatedMiddleName,
                Contact.Fields.dfeta_StatedLastName,
                Contact.Fields.dfeta_TRNAllocateRequest,
                Contact.Fields.dfeta_PreviousLastName
            });
        var orderedTeachers = teachers.OrderBy(x => x.dfeta_TRNAllocateRequest);

        // Assert
        Assert.Collection(orderedTeachers,
            teacher1 =>
            {
                Assert.Equal(teacher1Id, teacher1.Id);
                Assert.Equal(lastName, teacher1.LastName);
                Assert.Null(teacher1.dfeta_PreviousLastName);
            },
            teacher2 =>
            {
                Assert.Equal(teacher2Id, teacher2.Id);
                Assert.Equal(lastName, teacher2.dfeta_PreviousLastName);
            }
        );
    }
}
