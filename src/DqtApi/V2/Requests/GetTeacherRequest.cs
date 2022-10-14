using DqtApi.V2.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DqtApi.V2.Requests
{
    public class GetTeacherRequest : IRequest<GetTeacherResponse>
    {
        [FromRoute(Name = "trn")]
        public string Trn { get; set; }
    }
}
