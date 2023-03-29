using System;
using MediatR;

namespace QualifiedTeachersApi.Services.GetAnIdentity.WebHooks;

public class UserUpdatedRequest : IRequest
{
    public Guid UserId { get; set; }
    public string Trn { get; init; }
    public string EmailAddress { get; set; }
    public string MobileNumber { get; set; }
    public DateTime UpdateTimeUtc { get; set; }
}
