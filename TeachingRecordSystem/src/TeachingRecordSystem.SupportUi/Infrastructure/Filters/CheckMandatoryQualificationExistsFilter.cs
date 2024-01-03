using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Filters;

/// <summary>
/// Checks that a Mandatory Qualification exists with the ID specified by the qualificationId route value.
/// </summary>
/// <remarks>
/// <para>Returns a <see cref="StatusCodes.Status400BadRequest"/> response if the request is missing the qualicationId route value.</para>
/// <para>Returns a <see cref="StatusCodes.Status404NotFound"/> response if no Mandatory Qualification with the specified ID exists.</para>
/// <para>Assigns the <see cref="CurrentMandatoryQualificationFeature"/> and <see cref="CurrentPersonFeature"/> on success.</para>
/// </remarks>
public class CheckMandatoryQualificationExistsFilter(ICrmQueryDispatcher crmQueryDispatcher, ReferenceDataCache referenceDataCache) :
    AssignCurrentPersonInfoFilterBase(crmQueryDispatcher), IAsyncResourceFilter
{
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        if (context.RouteData.Values["qualificationId"] is not string qualificationIdParam ||
            !Guid.TryParse(qualificationIdParam, out Guid qualificationId))
        {
            context.Result = new BadRequestResult();
            return;
        }

        var qualification = await CrmQueryDispatcher.ExecuteQuery(new GetQualificationByIdQuery(qualificationId));

        if (qualification is null ||
            qualification.dfeta_Type != Core.Dqt.Models.dfeta_qualification_dfeta_Type.MandatoryQualification)
        {
            context.Result = new NotFoundResult();
            return;
        }

        var mq = await MandatoryQualification.MapFromDqtQualification(qualification, referenceDataCache);

        var provider = mq.ProviderId is Guid providerId ?
            MandatoryQualificationProvider.All.Single(p => p.MandatoryQualificationProviderId == providerId) :
            null;

        var dqtEstablishment = qualification.dfeta_MQ_MQEstablishmentId?.Id is Guid establishmentId ?
            await referenceDataCache.GetMqEstablishmentById(establishmentId) :
            null;

        context.HttpContext.SetCurrentMandatoryQualificationFeature(new(mq, provider, dqtEstablishment?.dfeta_name, dqtEstablishment?.dfeta_Value));

        await TryAssignCurrentPersonInfo(qualification.dfeta_PersonId!.Id, context.HttpContext);

        await next();
    }
}
