using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Filters;

public abstract class AssignCurrentPersonInfoFilterBase(ICrmQueryDispatcher crmQueryDispatcher)
{
    protected ICrmQueryDispatcher CrmQueryDispatcher { get; } = crmQueryDispatcher;

    protected async Task<bool> TryAssignCurrentPersonInfo(Guid personId, HttpContext httpContext)
    {
        var contactDetail = await CrmQueryDispatcher.ExecuteQuery(
            new GetActiveContactDetailByIdQuery(
                personId,
                new ColumnSet(
                    Contact.Fields.Id,
                    Contact.Fields.FirstName,
                    Contact.Fields.MiddleName,
                    Contact.Fields.LastName)));

        if (contactDetail is null)
        {
            return false;
        }

        httpContext.SetCurrentPersonFeature(
            new CurrentPersonFeature(
                contactDetail.Contact.Id,
                contactDetail.Contact.FirstName,
                contactDetail.Contact.MiddleName ?? "",
                contactDetail.Contact.LastName));
        return true;
    }
}
