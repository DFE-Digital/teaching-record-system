using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using QualifiedTeachersApi.V3.Responses;

namespace QualifiedTeachersApi.V3.Requests;

public record GetNpqCertificateRequest : IRequest<GetCertificateResponse?>
{
    [FromRoute(Name = "qualificationId")]
    [Description("The ID of the qualification record associated with the certificate.")]
    public required Guid QualificationId { get; set; }
}
