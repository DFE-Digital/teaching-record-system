using DqtApi.V2.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DqtApi.V2.Requests
{
    public class GetTeacherIdentityInfoByTsPersonIdRequest : IRequest<TeacherIdentityInfo>
    {
        [FromQuery(Name = "tsPersonId")]
        public string TsPersonId { get; set; }
    }
}
