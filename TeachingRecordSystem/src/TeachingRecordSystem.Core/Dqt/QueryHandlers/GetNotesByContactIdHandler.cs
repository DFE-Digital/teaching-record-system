using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetNotesByContactIdHandler : ICrmQueryHandler<GetNotesByContactIdQuery, TeacherNotesResult>
{
    public async Task<TeacherNotesResult> Execute(GetNotesByContactIdQuery query, IOrganizationServiceAsync organizationService)
    {
        var annotationFilter = new FilterExpression();
        annotationFilter.AddCondition(Annotation.Fields.ObjectId, ConditionOperator.Equal, query.ContactId);
        annotationFilter.AddCondition(Annotation.Fields.ObjectTypeCode, ConditionOperator.Equal, Contact.EntityLogicalName);
        var annotationQueryExpression = new QueryExpression(Annotation.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(
                Annotation.PrimaryIdAttribute,
                Annotation.Fields.Subject,
                Annotation.Fields.NoteText,
                Annotation.Fields.ModifiedBy,
                Annotation.Fields.ModifiedOn),
            Criteria = annotationFilter
        };

        var annotationUserLink = annotationQueryExpression.AddLink(
            SystemUser.EntityLogicalName,
            Annotation.Fields.ModifiedBy,
            SystemUser.PrimaryIdAttribute,
            JoinOperator.Inner);
        annotationUserLink.Columns = new ColumnSet(
            SystemUser.PrimaryIdAttribute,
            SystemUser.Fields.FirstName,
            SystemUser.Fields.LastName);
        annotationUserLink.EntityAlias = SystemUser.EntityLogicalName;

        var annotationRequest = new RetrieveMultipleRequest()
        {
            Query = annotationQueryExpression
        };

        var incidentFilter = new FilterExpression();
        incidentFilter.AddCondition(Incident.Fields.CustomerId, ConditionOperator.Equal, query.ContactId);
        var incidentQueryExpression = new QueryExpression(Incident.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(
                Incident.PrimaryIdAttribute,
                Incident.Fields.Title),
            Criteria = incidentFilter
        };

        var incidentResolutionLink = incidentQueryExpression.AddLink(
            IncidentResolution.EntityLogicalName,
            Incident.PrimaryIdAttribute,
            IncidentResolution.Fields.IncidentId,
            JoinOperator.Inner);
        incidentResolutionLink.Columns = new ColumnSet(
            IncidentResolution.PrimaryIdAttribute,
            IncidentResolution.Fields.Subject,
            IncidentResolution.Fields.CreatedBy,
            IncidentResolution.Fields.ModifiedBy,
            IncidentResolution.Fields.ModifiedOn,
            IncidentResolution.Fields.StateCode);
        incidentResolutionLink.EntityAlias = IncidentResolution.EntityLogicalName;

        var incidentResolutionCreatedByUserLink = incidentResolutionLink.AddLink(
            SystemUser.EntityLogicalName,
            IncidentResolution.Fields.CreatedBy,
            SystemUser.PrimaryIdAttribute,
            JoinOperator.Inner);
        incidentResolutionCreatedByUserLink.Columns = new ColumnSet(
            SystemUser.PrimaryIdAttribute,
            SystemUser.Fields.FirstName,
            SystemUser.Fields.LastName);
        incidentResolutionCreatedByUserLink.EntityAlias = $"{IncidentResolution.EntityLogicalName}.{SystemUser.EntityLogicalName}_createdby";

        var incidentResolutionModifiedByUserLink = incidentResolutionLink.AddLink(
            SystemUser.EntityLogicalName,
            IncidentResolution.Fields.ModifiedBy,
            SystemUser.PrimaryIdAttribute,
            JoinOperator.Inner);
        incidentResolutionModifiedByUserLink.Columns = new ColumnSet(
            SystemUser.PrimaryIdAttribute,
            SystemUser.Fields.FirstName,
            SystemUser.Fields.LastName);
        incidentResolutionModifiedByUserLink.EntityAlias = $"{IncidentResolution.EntityLogicalName}.{SystemUser.EntityLogicalName}_modifiedby";

        var incidentRequest = new RetrieveMultipleRequest()
        {
            Query = incidentQueryExpression
        };

        var taskFilter = new FilterExpression();
        taskFilter.AddCondition(CrmTask.Fields.RegardingObjectId, ConditionOperator.Equal, query.ContactId);
        var taskQueryExpression = new QueryExpression(CrmTask.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(
                CrmTask.PrimaryIdAttribute,
                CrmTask.Fields.Subject,
                CrmTask.Fields.Description,
                CrmTask.Fields.ScheduledEnd,
                CrmTask.Fields.ModifiedBy,
                CrmTask.Fields.ModifiedOn,
                CrmTask.Fields.StateCode),
            Criteria = taskFilter
        };

        var taskUserLink = taskQueryExpression.AddLink(
            SystemUser.EntityLogicalName,
            CrmTask.Fields.ModifiedBy,
            SystemUser.PrimaryIdAttribute,
            JoinOperator.Inner);
        taskUserLink.Columns = new ColumnSet(
            SystemUser.PrimaryIdAttribute,
            SystemUser.Fields.FirstName,
            SystemUser.Fields.LastName);
        taskUserLink.EntityAlias = SystemUser.EntityLogicalName;

        var taskRequest = new RetrieveMultipleRequest()
        {
            Query = taskQueryExpression
        };

        var requestBuilder = RequestBuilder.CreateMultiple(organizationService);
        var annotationResponse = requestBuilder.AddRequest<RetrieveMultipleResponse>(annotationRequest);
        var incidentResponse = requestBuilder.AddRequest<RetrieveMultipleResponse>(incidentRequest);
        var taskResponse = requestBuilder.AddRequest<RetrieveMultipleResponse>(taskRequest);

        await requestBuilder.Execute();

        var annotations = (await annotationResponse.GetResponseAsync()).EntityCollection.Entities.Select(e => e.ToEntity<Annotation>()).ToArray();
        var incidents = (await incidentResponse.GetResponseAsync()).EntityCollection.Entities.Select(e => e.ToEntity<Incident>()).ToArray();
        var incidentResolutions = incidents.Select(i => (i.Extract<IncidentResolution>(IncidentResolution.EntityLogicalName, IncidentResolution.PrimaryIdAttribute), i)).ToArray();
        var tasks = (await taskResponse.GetResponseAsync()).EntityCollection.Entities.Select(e => e.ToEntity<CrmTask>()).ToArray();

        return new TeacherNotesResult(annotations, incidentResolutions, tasks);
    }
}
