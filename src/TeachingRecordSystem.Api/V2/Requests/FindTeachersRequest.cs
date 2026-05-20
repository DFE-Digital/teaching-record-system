#nullable disable
using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Api.V2.Responses;

namespace TeachingRecordSystem.Api.V2.Requests;

public enum FindTeachersMatchPolicy
{
    Default = 0,
    Strict = 1
}

public class FindTeachersRequest : IRequest<FindTeachersResponse>
{
    [FromQuery(Name = "matchPolicy")]
    public FindTeachersMatchPolicy? MatchPolicy { get; set; }

    [Description("First name of person")]
    [FromQuery(Name = "firstName")]
    public string FirstName { get; set; }

    [Description("Last name of person")]
    [FromQuery(Name = "lastName")]
    public string LastName { get; set; }

    [Description("Previous first name of person")]
    [FromQuery(Name = "previousFirstName")]
    public string PreviousFirstName { get; set; }

    [Description("Previous last name of person")]
    [FromQuery(Name = "previousLastName")]
    public string PreviousLastName { get; set; }

    [FromQuery(Name = "dateOfBirth")]
    [Description("Date of birth of person")]
    public DateOnly? DateOfBirth { get; set; }

    [Description("National insurance number of person")]
    [FromQuery(Name = "nationalInsuranceNumber")]
    public string NationalInsuranceNumber { get; set; }

    [Description("Name of teacher training provider")]
    [FromQuery(Name = "ittProviderName")]
    public string IttProviderName { get; set; }

    [Description("UKPRN of teacher training provider")]
    [FromQuery(Name = "ittProviderUkprn")]
    public string IttProviderUkprn { get; set; }

    [Description("Email of person")]
    [FromQuery(Name = "emailAddress")]
    public string EmailAddress { get; set; }

    [Description("TRN of person")]
    [FromQuery(Name = "trn")]
    public string Trn { get; set; }
}
