using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.DataStore.Crm.Models;
using QualifiedTeachersApi.DataStore.Sql;
using QualifiedTeachersApi.Services;
using QualifiedTeachersApi.V2.ApiModels;
using QualifiedTeachersApi.V2.Requests;
using QualifiedTeachersApi.Validation;

namespace QualifiedTeachersApi.V2.Handlers
{
    public class SetNpqQualificationHandler : IRequestHandler<SetNpqQualificationRequest>
    {
        private readonly IDataverseAdapter _dataverseAdapter;

        public SetNpqQualificationHandler(IDataverseAdapter dataverseAdapter)
        {
            _dataverseAdapter = dataverseAdapter;
        }

        public async Task<Unit> Handle(SetNpqQualificationRequest request, CancellationToken cancellationToken)
        {
            var contacts = await _dataverseAdapter.GetTeachersByTrn(request.Trn, columnNames: new[]
                {
                    Contact.Fields.FirstName,
                    Contact.Fields.LastName,
                    Contact.Fields.BirthDate,
                    Contact.Fields.dfeta_EYTSDate,
                    Contact.Fields.dfeta_QTSDate,
                    Contact.Fields.dfeta_ActiveSanctions,
                    Contact.Fields.dfeta_NINumber,
                    Contact.Fields.dfeta_TRN
                },
                activeOnly: true);

            if (contacts.Length == 0)
            {
                throw new ErrorException(ErrorRegistry.TeacherWithSpecifiedTrnNotFound());
            }
            else if (contacts.Length > 1)
            {
                throw new ErrorException(ErrorRegistry.MultipleTeachersFoundWithSpecifiedTrn());
            }

            var setNpqQualificationResult = await _dataverseAdapter.SetNpqQualification(new SetNpqQualificationCommand()
            {
                TeacherId = contacts[0].Id,
                CompletionDate = request.CompletionDate.ToDateTime(),
                QualificationType = ((dfeta_qualification_dfeta_Type)(request.QualificationType?.ConvertToQualificationType()))
            }); ;

            if (!setNpqQualificationResult.Succeeded)
            {
                throw CreateValidationExceptionFromFailedReasons(setNpqQualificationResult.FailedReasons);
            }

            return Unit.Value;
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
}
