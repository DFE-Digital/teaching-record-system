using MediatR;
using Microsoft.AspNetCore.Mvc;
using QualifiedTeachersApi.V3.Responses;

namespace QualifiedTeachersApi.V3.Requests;

public record GetQtsCertificateRequest : IRequest<FileResult>
{
    public required string Trn { get; init; }
}
