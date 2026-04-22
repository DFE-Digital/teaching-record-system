#pragma warning disable TRS0001
using CoreTrnRequestInfo = TeachingRecordSystem.Api.V3.Implementation.Dtos.TrnRequestInfo;

namespace TeachingRecordSystem.Core.ApiSchema.V3.V20240307.Dtos;

public static class TrnRequestInfoExtensions
{
    public static TrnRequestInfo FromModel(this CoreTrnRequestInfo model) => new()
    {
        RequestId = model.RequestId,
        Person = new TrnRequestPerson
        {
            FirstName = model.Person.FirstName,
            MiddleName = model.Person.MiddleName,
            LastName = model.Person.LastName,
            DateOfBirth = model.Person.DateOfBirth,
            Email = model.Person.EmailAddress,
            NationalInsuranceNumber = model.Person.NationalInsuranceNumber
        },
        Status = (TrnRequestStatus)(int)model.Status,
        Trn = model.Trn
    };
}
#pragma warning restore TRS0001
