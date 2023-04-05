﻿#nullable disable
using FluentValidation;
using QualifiedTeachersApi.V2.Requests;

namespace QualifiedTeachersApi.V2.Validators;

public class GetTeacherRequestValidator : AbstractValidator<GetTeacherRequest>
{
    public GetTeacherRequestValidator()
    {
        RuleFor(x => x.Trn)
            .Matches(@"^\d{7}$")
            .WithMessage(Properties.StringResources.ErrorMessages_TRNMustBe7Digits);
    }
}
