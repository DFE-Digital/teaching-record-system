using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt;
public partial class DataverseAdapter
{
    public async Task<dfeta_document?> GetDocumentById(Guid documentId)
    {
        var filter = new FilterExpression();
        filter.AddCondition(dfeta_document.PrimaryIdAttribute, ConditionOperator.Equal, documentId);

        var queryExpression = new QueryExpression(dfeta_document.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(
                dfeta_document.PrimaryIdAttribute,
                dfeta_document.Fields.dfeta_name),
            Criteria = filter
        };
        AddAnnotationLink(queryExpression);

        var result = await _service.RetrieveMultipleAsync(queryExpression);
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
                Annotation.Fields.FileName);

            annotationLink.EntityAlias = Annotation.EntityLogicalName;
        }
    }
}
