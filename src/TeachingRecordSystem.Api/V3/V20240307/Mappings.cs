#pragma warning disable TRS0001
using TeachingRecordSystem.Core.ApiSchema.V3.V20240307.Dtos;
using Common = TeachingRecordSystem.Api.V3.Operations.Common;
using Dtos = TeachingRecordSystem.Core.ApiSchema.V3.V20240307.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240307;

public static class TrnRequestInfoMappingExtensions
{
    extension(Dtos.TrnRequestInfo)
    {
        public static Dtos.TrnRequestInfo Create(Common.TrnRequestInfo source) => new()
        {
            RequestId = source.RequestId,
            Person = Dtos.TrnRequestPerson.Create(source.Person),
            Status = Dtos.TrnRequestStatus.Create(source.Status),
            Trn = source.Trn
        };
    }
}

public static class TrnRequestPersonMappingExtensions
{
    extension(Dtos.TrnRequestPerson)
    {
        public static Dtos.TrnRequestPerson Create(Common.TrnRequestInfoPerson source) => new()
        {
            FirstName = source.FirstName,
            MiddleName = source.MiddleName,
            LastName = source.LastName,
            DateOfBirth = source.DateOfBirth,
            Email = source.EmailAddress,
            NationalInsuranceNumber = source.NationalInsuranceNumber
        };
    }
}
