using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Filters;

public class CheckPersonCanBeMergedFilter(TrsDbContext dbContext) : IAsyncResourceFilter
{
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var currentPerson = context.HttpContext.Features.GetRequiredFeature<CurrentPersonFeature>();
        var person = await dbContext.Persons
            .SingleAsync(p => p.PersonId == currentPerson.PersonId);

        var hasOpenAlert = await dbContext.Alerts
            .AnyAsync(a => a.PersonId == currentPerson.PersonId && a.EndDate == null);

        var hasMandatoryQualification = await dbContext.MandatoryQualifications
            .AnyAsync(mq => mq.PersonId == currentPerson.PersonId);

        var hasProfessionalStatus =
            person.EytsDate.HasValue ||
            person.HasEyps ||
            person.PqtsDate.HasValue ||
            person.QtsDate.HasValue ||
            person.QtlsStatus != QtlsStatus.None;

        if (hasOpenAlert ||
            hasMandatoryQualification ||
            hasProfessionalStatus ||
            person.InductionStatus != InductionStatus.None)
        {
            context.Result = new BadRequestResult();
            return;
        }

        await next();
    }
}
