using System;
using FluentValidation;
using QualifiedTeachersApi.DataStore.Crm.Models;
using QualifiedTeachersApi.DataStore.Sql.Models;
using QualifiedTeachersApi.Properties;
using QualifiedTeachersApi.V2.ApiModels;
using QualifiedTeachersApi.V2.Requests;

namespace QualifiedTeachersApi.V2.Validators;

public class GetOrCreateTrnRequestValidator : AbstractValidator<GetOrCreateTrnRequest>
{
    public GetOrCreateTrnRequestValidator(IClock clock)
    {
        RuleFor(r => r.RequestId)
            .Matches(TrnRequest.ValidRequestIdPattern)
            .WithMessage(Properties.StringResources.ErrorMessages_RequestIdCanOnlyContainCharactersDigitsUnderscoresAndDashes)
            .MaximumLength(TrnRequest.RequestIdMaxLength)
            .WithMessage(Properties.StringResources.ErrorMessages_RequestIdMustBe100CharactersOrFewer);

        RuleFor(r => r.FirstName)
            .NotEmpty()
            .MaximumLength(AttributeConstraints.Contact.FirstNameMaxLength);

        RuleFor(r => r.MiddleName)
            .MaximumLength(AttributeConstraints.Contact.MiddleNameMaxLength);

        RuleFor(r => r.LastName)
            .NotEmpty()
            .MaximumLength(AttributeConstraints.Contact.LastNameMaxLength);

        RuleFor(r => r.BirthDate)
            .NotEmpty()
            .Custom((value, ctx) =>
            {
                if (value >= DateOnly.FromDateTime(clock.UtcNow) || value < new DateOnly(1940, 1, 1))
                {
                    ctx.AddFailure(ctx.PropertyName, StringResources.ErrorMessages_BirthDateIsOutOfRange);
                }
            });

        RuleFor(r => r.EmailAddress)
            .EmailAddress()
            .MaximumLength(AttributeConstraints.Contact.EMailAddress1MaxLength);

        RuleFor(r => r.Address.AddressLine1)
            .MaximumLength(AttributeConstraints.Contact.Address1_Line1MaxLength)
            .When(r => r.Address != null);

        RuleFor(r => r.Address.AddressLine2)
            .MaximumLength(AttributeConstraints.Contact.Address1_Line2MaxLength)
            .When(r => r.Address != null);

        RuleFor(r => r.Address.AddressLine3)
            .MaximumLength(AttributeConstraints.Contact.Address1_Line3MaxLength)
            .When(r => r.Address != null);

        RuleFor(r => r.Address.City)
            .MaximumLength(AttributeConstraints.Contact.Address1_CityMaxLength)
            .When(r => r.Address != null);

        RuleFor(r => r.Address.Country)
            .MaximumLength(AttributeConstraints.Contact.Address1_CountryMaxLength)
            .When(r => r.Address != null);

        RuleFor(r => r.Address.PostalCode)
            .MaximumLength(AttributeConstraints.Contact.Address1_PostalCodeLength)
            .When(r => r.Address != null);

        RuleFor(r => r.GenderCode)
            .IsInEnum();

        RuleFor(r => r.InitialTeacherTraining)
            .NotNull();

        RuleFor(r => r.InitialTeacherTraining.ProviderUkprn)
            .NotEmpty()
            .When(r => r.InitialTeacherTraining != null && r.TeacherType == CreateTeacherType.TraineeTeacher);

        RuleFor(r => r.InitialTeacherTraining.ProgrammeStartDate)
            .NotNull()
            .When(r => r.InitialTeacherTraining != null);

        RuleFor(r => r.InitialTeacherTraining.ProgrammeEndDate)
            .NotNull()
            .When(r => r.InitialTeacherTraining != null);

        RuleFor(r => r.InitialTeacherTraining.ProgrammeType)
            .Cascade(CascadeMode.Stop)
            .IsInEnum()
            .When(r => r.InitialTeacherTraining != null, ApplyConditionTo.CurrentValidator)
            .NotNull()
            .When(r => r.InitialTeacherTraining != null && r.TeacherType == CreateTeacherType.TraineeTeacher, ApplyConditionTo.CurrentValidator)
            .Must(pt => !pt.HasValue || !pt.Value.ConvertToIttProgrammeType().IsEarlyYears())
            .When(r => r.InitialTeacherTraining != null && r.TeacherType == CreateTeacherType.OverseasQualifiedTeacher, ApplyConditionTo.CurrentValidator);

        RuleFor(r => r.InitialTeacherTraining.IttQualificationAim)
            .IsInEnum()
            .When(r => r.InitialTeacherTraining != null);

        RuleFor(r => r.InitialTeacherTraining.AgeRangeFrom)
            .Must(v => AgeRange.TryConvertFromValue(v.Value, out _))
            .When(r => r.InitialTeacherTraining?.AgeRangeFrom.HasValue == true)
            .WithMessage(StringResources.ErrorMessages_AgeMustBe0To19Inclusive);

        RuleFor(r => r.InitialTeacherTraining.AgeRangeTo)
            .Custom((value, ctx) =>
            {
                if (!value.HasValue)
                {
                    return;
                }

                if (!AgeRange.TryConvertFromValue(value.Value, out var ageTo))
                {
                    ctx.AddFailure(ctx.PropertyName, StringResources.ErrorMessages_AgeMustBe0To19Inclusive);
                    return;
                }

                if (ctx.InstanceToValidate.InitialTeacherTraining.AgeRangeFrom.HasValue &&
                    ctx.InstanceToValidate.InitialTeacherTraining.AgeRangeFrom.Value > value)
                {
                    ctx.AddFailure(ctx.PropertyName, StringResources.ErrorMessages_AgeToCannotBeLessThanAgeFrom);
                }
            })
            .When(r => r.InitialTeacherTraining != null);

        RuleFor(r => r.InitialTeacherTraining.IttQualificationType)
            .IsInEnum()
            .When(r => r.InitialTeacherTraining != null)
            .Must(qt => qt != IttQualificationType.InternationalQualifiedTeacherStatus)
            .When(r => r.InitialTeacherTraining != null && r.InitialTeacherTraining.ProgrammeType != IttProgrammeType.InternationalQualifiedTeacherStatus);

        Func<GetOrCreateTrnRequest, bool> trainingCountryRequired = r =>
            r.TeacherType == CreateTeacherType.OverseasQualifiedTeacher || r.InitialTeacherTraining.ProgrammeType == IttProgrammeType.InternationalQualifiedTeacherStatus;

        RuleFor(r => r.InitialTeacherTraining.TrainingCountryCode)
            .Empty()
            .When(r => !trainingCountryRequired(r), ApplyConditionTo.CurrentValidator)
            .NotEmpty()
            .When(trainingCountryRequired, ApplyConditionTo.CurrentValidator)
            .When(r => r.InitialTeacherTraining != null, ApplyConditionTo.AllValidators);

        RuleFor(r => r.InitialTeacherTraining.TrainingCountryCode)
            .Custom((countryCode, ctx) =>
            {
                if (ctx.InstanceToValidate.TeacherType == CreateTeacherType.OverseasQualifiedTeacher &&
                    !string.IsNullOrEmpty(ctx.InstanceToValidate.InitialTeacherTraining?.TrainingCountryCode))
                {
                    var route = ctx.InstanceToValidate.RecognitionRoute;
                    var countryCodePropertyName = $"{nameof(ctx.InstanceToValidate.InitialTeacherTraining)}.{nameof(ctx.InstanceToValidate.InitialTeacherTraining.TrainingCountryCode)}";

                    if (countryCode == "XK")
                    {
                        ctx.AddFailure(
                            countryCodePropertyName,
                            $"CountryCode cannot be 'XK' when TeacherType is '{CreateTeacherType.OverseasQualifiedTeacher}'.");

                        return;
                    }

                    if (route is CreateTeacherRecognitionRoute.Scotland)
                    {
                        if (countryCode != "XH")
                        {
                            ctx.AddFailure(
                                countryCodePropertyName,
                                $"CountryCode must be 'XH' when RecognitionRoute is '{CreateTeacherRecognitionRoute.Scotland}'.");
                        }
                    }
                    else if (route is CreateTeacherRecognitionRoute.NorthernIreland)
                    {
                        if (countryCode != "XG")
                        {
                            ctx.AddFailure(
                                countryCodePropertyName,
                                $"CountryCode must be 'XG' when RecognitionRoute is '{CreateTeacherRecognitionRoute.NorthernIreland}'.");
                        }
                    }
                    else
                    {
                        if (countryCode == "XH" || countryCode == "XG")
                        {
                            ctx.AddFailure(
                                countryCodePropertyName,
                                $"CountryCode cannot be 'XH' or 'XG' when RecognitionRoute is '{CreateTeacherRecognitionRoute.OverseasTrainedTeachers}'.");
                        }
                    }
                }
            });

        RuleFor(r => r.Qualification.Class)
            .IsInEnum()
            .When(r => r.Qualification != null);

        RuleFor(r => r.Qualification.HeQualificationType)
            .IsInEnum()
            .When(r => r.Qualification != null);

        RuleFor(r => r.RecognitionRoute)
            .NotNull()
            .When(r => r.TeacherType == CreateTeacherType.OverseasQualifiedTeacher, ApplyConditionTo.CurrentValidator)
            .Null()
            .When(r => r.TeacherType != CreateTeacherType.OverseasQualifiedTeacher, ApplyConditionTo.CurrentValidator);

        RuleFor(r => r.QtsDate)
            .NotNull()
            .When(r => r.TeacherType == CreateTeacherType.OverseasQualifiedTeacher, ApplyConditionTo.CurrentValidator)
            .Null()
            .When(r => r.TeacherType != CreateTeacherType.OverseasQualifiedTeacher, ApplyConditionTo.CurrentValidator);

        RuleFor(r => r.InductionRequired)
            .NotNull()
            .When(r => r.TeacherType == CreateTeacherType.OverseasQualifiedTeacher, ApplyConditionTo.CurrentValidator)
            .Null()
            .When(r => r.TeacherType != CreateTeacherType.OverseasQualifiedTeacher, ApplyConditionTo.CurrentValidator);
    }
}
