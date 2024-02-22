using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public async Task<PersonEmployment> CreatePersonEmployment(
        Guid personId,
        Guid establishmentId,
        DateOnly startDate,
        EmploymentType employmentType,
        DateOnly? endDate = null)
    {
        var personEmployment = await WithDbContext(async dbContext =>
        {
            var personEmployment = new PersonEmployment
            {
                PersonEmploymentId = Guid.NewGuid(),
                PersonId = personId,
                EstablishmentId = establishmentId,
                StartDate = startDate,
                EndDate = endDate,
                EmploymentType = employmentType,
                CreatedOn = Clock.UtcNow,
                UpdatedOn = Clock.UtcNow
            };

            dbContext.PersonEmployments.Add(personEmployment);
            await dbContext.SaveChangesAsync();

            return personEmployment;
        });

        return personEmployment;
    }
}
