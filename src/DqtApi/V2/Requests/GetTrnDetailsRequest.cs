using DqtApi.V2.Responses;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;

namespace DqtApi.V2.Requests
{
    public class GetTrnDetailsRequest : IRequest<GetTrnDetailsResponse>
    {
        [SwaggerParameter(Description = "Email Address")]
        public string EmailAddress { get; set; }

        [SwaggerParameter(Description = "First name of person")]
        public string FirstName { get; set; }

        [SwaggerParameter(Description = "Middle name of person")]
        public string MiddleName { get; set; }

        [SwaggerParameter(Description = "Last name of person")]
        public string LastName { get; set; }

        [SwaggerParameter(Description = "Previous first name of person")]
        public string PreviousFirstName { get; set; }

        [SwaggerParameter(Description = "Previous last name of person")]
        public string PreviousLastName { get; set; }

        [SwaggerParameter(Description = "dob of person")]
        public string DateOfBirth { get; set; }

        [SwaggerParameter(Description = "Nino of person")]
        public string Nino { get; set; }

        [SwaggerParameter(Description = "Name of teacher training provider")]
        public string IttProviderName { get; set; }

        [SwaggerParameter(Description = "Ukprn of itt provider")]
        public string IttProviderUkprn { get; set; }
    }
}
