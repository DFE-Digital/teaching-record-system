using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.SupportUi.Endpoints;

public static class CaseEndpoints
{
    public static IEndpointConventionBuilder MapCaseEndpoints(this IEndpointRouteBuilder builder)
    {
        return builder.MapGroup("/cases")
            .MapGet(
            "/{caseId}/documents/{documentId}",
            async (
                string caseId,
                Guid documentId,
                HttpContext httpContext,
                IDataverseAdapter dataverseAdapter) =>
            {
                var document = await dataverseAdapter.GetDocumentById(documentId);
                var annotation = document?.Extract<Annotation>("annotation", Annotation.PrimaryIdAttribute);

                if (document is null || annotation is null)
                {
                    return Results.NotFound();
                }

                if (document.StateCode != dfeta_documentState.Active)
                {
                    return Results.BadRequest();
                }

                return Results.Ok($"data:{annotation.MimeType};base64,{annotation.DocumentBody}");
            });

    }
}
