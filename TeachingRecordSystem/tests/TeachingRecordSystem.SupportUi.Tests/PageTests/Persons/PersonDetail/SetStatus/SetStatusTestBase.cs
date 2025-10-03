using static TeachingRecordSystem.TestCommon.TestData;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.SetStatus;

public class SetStatusTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected async Task<CreatePersonResult> CreatePersonWithCurrentStatus(PersonStatus currentStatus, Action<CreatePersonBuilder>? configure = null)
    {
        configure ??= p => { };

        var person = await TestData.CreatePersonAsync(p =>
        {
            p.WithPersonDataSource(TestDataPersonDataSource.Trs);
            configure(p);
        });

        if (currentStatus == PersonStatus.Deactivated)
        {
            await WithDbContext(async dbContext =>
            {
                dbContext.Attach(person.Person);
                person.Person.Status = PersonStatus.Deactivated;
                await dbContext.SaveChangesAsync();
            });
        }

        return person;
    }

    protected async Task<CreatePersonResult> CreatePersonToBecomeStatus(PersonStatus targetStatus, Action<CreatePersonBuilder>? configure = null)
    {
        configure ??= p => { };

        var person = await TestData.CreatePersonAsync(p =>
        {
            p.WithPersonDataSource(TestDataPersonDataSource.Trs);
            configure(p);
        });

        if (targetStatus == PersonStatus.Active)
        {
            await WithDbContext(async dbContext =>
            {
                dbContext.Attach(person.Person);
                person.Person.Status = PersonStatus.Deactivated;
                await dbContext.SaveChangesAsync();
            });
        }

        return person;
    }

    public static PersonStatus[] GetAllStatuses() =>
        [PersonStatus.Active, PersonStatus.Deactivated];
}
