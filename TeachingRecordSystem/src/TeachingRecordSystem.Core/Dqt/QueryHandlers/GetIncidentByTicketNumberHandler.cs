using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;
public class GetIncidentByTicketNumberHandler : ICrmQueryHandler<GetIncidentByTicketNumberQuery, Incident?>
{
    public async Task<Incident?> Execute(GetIncidentByTicketNumberQuery query, IOrganizationServiceAsync organizationService)
    {
        var filter = new FilterExpression(LogicalOperator.And);
        filter.AddCondition(Incident.Fields.TicketNumber, ConditionOperator.Equal, query.TicketNumber);

        var queryExpression = new QueryExpression(Incident.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(
                Incident.Fields.TicketNumber,
                Incident.Fields.Title,
                Incident.Fields.CreatedOn,
                Incident.Fields.dfeta_NewDateofBirth,
                Incident.Fields.dfeta_NewFirstName,
                Incident.Fields.dfeta_NewMiddleName,
                Incident.Fields.dfeta_NewLastName,
                Incident.Fields.dfeta_StatedFirstName,
                Incident.Fields.dfeta_StatedMiddleName,
                Incident.Fields.dfeta_StatedLastName,
                Incident.Fields.StateCode),
            Criteria = filter
        };

        AddContactLink(queryExpression);
        AddSubjectLink(queryExpression);
        AddDocumentLink(queryExpression);

        var result = await organizationService.RetrieveMultipleAsync(queryExpression);
        return result.Entities.Select(entity => entity.ToEntity<Incident>()).SingleOrDefault();

        static void AddContactLink(QueryExpression query)
        {
            var contactLink = query.AddLink(
                Contact.EntityLogicalName,
                Incident.Fields.CustomerId,
                Contact.PrimaryIdAttribute,
                JoinOperator.Inner);

            contactLink.Columns = new ColumnSet(
                Contact.PrimaryIdAttribute,
                Contact.Fields.BirthDate,
                Contact.Fields.FirstName,
                Contact.Fields.MiddleName,
                Contact.Fields.LastName,
                Contact.Fields.dfeta_StatedFirstName,
                Contact.Fields.dfeta_StatedMiddleName,
                Contact.Fields.dfeta_StatedLastName);

            contactLink.EntityAlias = Contact.EntityLogicalName;
        }

        static void AddSubjectLink(QueryExpression query)
        {
            var subjectLink = query.AddLink(
                Subject.EntityLogicalName,
                Incident.Fields.SubjectId,
                Subject.PrimaryIdAttribute,
                JoinOperator.Inner);

            subjectLink.Columns = new ColumnSet(
                Subject.PrimaryIdAttribute,
                Subject.Fields.Title);

            subjectLink.EntityAlias = Subject.EntityLogicalName;
        }

        static void AddDocumentLink(QueryExpression query)
        {
            var documentLink = query.AddLink(
                dfeta_document.EntityLogicalName,
                Incident.PrimaryIdAttribute,
                dfeta_document.Fields.dfeta_CaseId,
                JoinOperator.Inner);

            documentLink.Columns = new ColumnSet(
                dfeta_document.PrimaryIdAttribute,
                dfeta_document.Fields.dfeta_name,
                dfeta_document.Fields.dfeta_CaseId);
            documentLink.EntityAlias = dfeta_document.EntityLogicalName;

            var filter = new FilterExpression();
            filter.AddCondition(dfeta_document.Fields.StateCode, ConditionOperator.Equal, (int)dfeta_documentState.Active);
            documentLink.LinkCriteria = filter;

            AddAnnotationLink(documentLink);
        }

        static void AddAnnotationLink(LinkEntity documentLink)
        {
            var annotationLink = documentLink.AddLink(
                Annotation.EntityLogicalName,
                dfeta_document.PrimaryIdAttribute,
                Annotation.Fields.ObjectId,
                JoinOperator.Inner);

            annotationLink.Columns = new ColumnSet(
                Annotation.PrimaryIdAttribute,
                Annotation.Fields.ObjectId,
                Annotation.Fields.Subject,
                Annotation.Fields.DocumentBody,
                Annotation.Fields.MimeType,
                Annotation.Fields.FileName);

            annotationLink.EntityAlias = $"{dfeta_document.EntityLogicalName}.{Annotation.EntityLogicalName}";
        }
    }
}
