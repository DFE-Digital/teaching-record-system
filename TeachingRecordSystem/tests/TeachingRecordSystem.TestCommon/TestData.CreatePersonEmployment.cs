using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using Establishment = TeachingRecordSystem.Core.DataStore.Postgres.Models.Establishment;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public async Task<PersonEmployment> CreatePersonEmployment(
        CreatePersonResult person,
        Establishment establishment,
        DateOnly startDate,
        DateOnly lastKnownEmployedDate,
        EmploymentType employmentType,
        DateOnly lastExtractDate,
        string? nationalInsuranceNumber = null,
        string? personPostcode = null,
        DateOnly? endDate = null)
    {
        var key = $"{person.Trn}.{establishment.LaCode}.{establishment.EstablishmentNumber}.{startDate:yyyyMMdd}";

        var personEmployment = await WithDbContext(async dbContext =>
        {
            var personEmployment = new PersonEmployment
            {
                PersonEmploymentId = Guid.NewGuid(),
                PersonId = person.PersonId,
                EstablishmentId = establishment.EstablishmentId,
                StartDate = startDate,
                EndDate = endDate,
                EmploymentType = employmentType,
                LastKnownEmployedDate = lastKnownEmployedDate,
                LastExtractDate = lastExtractDate,
                NationalInsuranceNumber = nationalInsuranceNumber,
                PersonPostcode = personPostcode,
                CreatedOn = Clock.UtcNow,
                UpdatedOn = Clock.UtcNow,
                Key = key
            };

            dbContext.PersonEmployments.Add(personEmployment);
            await dbContext.SaveChangesAsync();

            return personEmployment;
        });

        return personEmployment;
    }
}
