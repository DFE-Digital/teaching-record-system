using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetDocumentByIdHandler : ICrmQueryHandler<GetDocumentByIdQuery, dfeta_document?>
{
    public async Task<dfeta_document?> Execute(GetDocumentByIdQuery query, IOrganizationServiceAsync organizationService)
    {
        var filter = new FilterExpression();
        filter.AddCondition(dfeta_document.PrimaryIdAttribute, ConditionOperator.Equal, query.DocumentId);

        var queryExpression = new QueryExpression(dfeta_document.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(
                dfeta_document.PrimaryIdAttribute,
                dfeta_document.Fields.dfeta_name,
                dfeta_document.Fields.StateCode),
            Criteria = filter
        };

        AddAnnotationLink(queryExpression);

        var result = await organizationService.RetrieveMultipleAsync(queryExpression);
        return result.Entities.Select(entity => entity.ToEntity<dfeta_document>()).SingleOrDefault();

        static void AddAnnotationLink(QueryExpression queryExpression)
        {
            var annotationLink = queryExpression.AddLink(
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
                Annotation.Fields.FileName,
                Annotation.Fields.IsDocument);

            annotationLink.EntityAlias = Annotation.EntityLogicalName;

            var annotationFilter = new FilterExpression();
            annotationFilter.AddCondition(Annotation.Fields.IsDocument, ConditionOperator.Equal, true);
            annotationLink.LinkCriteria = annotationFilter;
        }
    }
}
