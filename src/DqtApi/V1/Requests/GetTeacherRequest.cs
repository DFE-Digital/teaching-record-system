using System;
using DqtApi.V1.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DqtApi.V1.Requests
{
    public class GetTeacherRequest : IRequest<GetTeacherResponse>
    {
        [FromRoute(Name = "trn")]
        public string Trn { get; set; }

        [FromQuery(Name = "birthdate"), SwaggerParameter(Required = true), SwaggerSchema(Format = "date"), ModelBinder(typeof(ModelBinding.DateModelBinder))]
        public DateTime? BirthDate { get; set; }

        [FromQuery(Name = "nino")]
        public string NationalInsuranceNumber { get; set; }
    }
}
