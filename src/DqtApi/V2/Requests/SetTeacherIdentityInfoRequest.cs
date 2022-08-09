using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DqtApi.V2.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DqtApi.V2.Requests
{
    public class SetTeacherIdentityInfoRequest : IRequest<TeacherIdentityInfo>
    {
        [FromRoute(Name = "trn")]
        [JsonIgnore]
        public string Trn { get; set; }

        [Required]
        public string TsPersonId { get; set; }
    }
}
