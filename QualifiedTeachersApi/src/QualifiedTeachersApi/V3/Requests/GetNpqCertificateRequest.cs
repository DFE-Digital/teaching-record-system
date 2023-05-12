using MediatR;
using Microsoft.AspNetCore.Mvc;
using QualifiedTeachersApi.V3.Responses;
using Swashbuckle.AspNetCore.Annotations;

namespace QualifiedTeachersApi.V3.Requests;

public record GetNpqCertificateRequest : IRequest<GetCertificateResponse?>
{
    [FromRoute(Name = "qualificationId")]
    [SwaggerParameter(description: "The ID of the qualification record associated with the certificate")]
    public required Guid QualificationId { get; set; }
}
