#nullable disable
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using TeachingRecordSystem.Api.V2.ApiModels;
using TeachingRecordSystem.Api.V2.Requests;
using TeachingRecordSystem.Api.Validation;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.Api.V2.Handlers;

public class SetNpqQualificationHandler : IRequestHandler<SetNpqQualificationRequest>
{
    private readonly IDataverseAdapter _dataverseAdapter;

    public SetNpqQualificationHandler(IDataverseAdapter dataverseAdapter)
    {
        _dataverseAdapter = dataverseAdapter;
    }

    public async Task Handle(SetNpqQualificationRequest request, CancellationToken cancellationToken)
    {
        var contact = await _dataverseAdapter.GetTeacherByTrn(
            request.Trn,
            columnNames: new[]
            {
                Contact.Fields.FirstName,
                Contact.Fields.LastName,
                Contact.Fields.BirthDate,
                Contact.Fields.dfeta_EYTSDate,
                Contact.Fields.dfeta_QTSDate,
                Contact.Fields.dfeta_NINumber,
                Contact.Fields.dfeta_TRN
            },
            activeOnly: true);

        if (contact is null)
        {
            throw new ErrorException(ErrorRegistry.TeacherWithSpecifiedTrnNotFound());
        }

        var setNpqQualificationResult = await _dataverseAdapter.SetNpqQualification(new SetNpqQualificationCommand()
        {
            TeacherId = contact.Id,
            CompletionDate = request.CompletionDate.ToDateTime(),
            QualificationType = ((dfeta_qualification_dfeta_Type)(request.QualificationType?.ConvertToQualificationType()))
        });

        if (!setNpqQualificationResult.Succeeded)
        {
            throw CreateValidationExceptionFromFailedReasons(setNpqQualificationResult.FailedReasons);
        }
    }

    private ValidationException CreateValidationExceptionFromFailedReasons(SetNpqQualificationFailedReasons failedReasons)
    {
        var failures = new List<ValidationFailure>();

        ConsumeReason(
            SetNpqQualificationFailedReasons.MultipleNpqQualificationsWithQualificationType,
            $"{nameof(SetNpqQualificationRequest.QualificationType)}",
            ErrorRegistry.MultipleNpqQualificationWithQualificationType().Title);

        ConsumeReason(
            SetNpqQualificationFailedReasons.NpqQualificationNotCreatedByApi,
            $"{nameof(SetNpqQualificationRequest.QualificationType)}",
            ErrorRegistry.NpqQualificationNotCreatedByApi().Title);

        if (failedReasons != SetNpqQualificationFailedReasons.None)
        {
            throw new NotImplementedException($"Unknown {nameof(SetNpqQualificationFailedReasons)}: '{failedReasons}.");
        }

        return new ValidationException(failures);

        void ConsumeReason(SetNpqQualificationFailedReasons reason, string propertyName, string message)
        {
            if (failedReasons.HasFlag(reason))
            {
                failures.Add(new ValidationFailure(propertyName, message));
                failedReasons = failedReasons & ~reason;
            }
        }
    }
}
