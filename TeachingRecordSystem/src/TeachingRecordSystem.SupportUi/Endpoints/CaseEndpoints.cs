using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Endpoints;

public static class CaseEndpoints
{
    public static IEndpointConventionBuilder MapCaseEndpoints(this IEndpointRouteBuilder builder)
    {
        return builder.MapGroup("/cases")
            .MapGet(
            "/{ticketNumber}/documents/{documentId}",
            async (
                string ticketNumber,
                Guid documentId,
                HttpContext httpContext,
                ICrmQueryDispatcher crmQueryDispatcher) =>
            {
                var document = await crmQueryDispatcher.ExecuteQuery(new GetDocumentByIdQuery(documentId));
                var annotation = document?.Extract<Annotation>("annotation", Annotation.PrimaryIdAttribute);

                if (document is null || annotation is null)
                {
                    return Results.NotFound();
                }

                if (document.StateCode != dfeta_documentState.Active)
                {
                    return Results.BadRequest();
                }

                var bytes = Convert.FromBase64String(annotation.DocumentBody);
                return Results.Bytes(bytes, annotation.MimeType);
            });
    }
}
