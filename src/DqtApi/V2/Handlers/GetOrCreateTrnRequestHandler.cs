﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DqtApi.DataStore.Crm;
using DqtApi.DataStore.Crm.Models;
using DqtApi.DataStore.Sql;
using DqtApi.DataStore.Sql.Models;
using DqtApi.Security;
using DqtApi.Services;
using DqtApi.V2.ApiModels;
using DqtApi.V2.Requests;
using DqtApi.V2.Responses;
using DqtApi.Validation;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DqtApi.V2.Handlers
{
    public class GetOrCreateTrnRequestHandler : IRequestHandler<GetOrCreateTrnRequest, TrnRequestInfo>
    {
        private static readonly TimeSpan _lockTimeout = TimeSpan.FromMinutes(1);

        private readonly DqtContext _dqtContext;
        private readonly IDataverseAdapter _dataverseAdapter;
        private readonly ICurrentClientProvider _currentClientProvider;
        private readonly IDistributedLockService _distributedLockService;

        public GetOrCreateTrnRequestHandler(
            DqtContext dqtContext,
            IDataverseAdapter dataverseAdapter,
            ICurrentClientProvider currentClientProvider,
            IDistributedLockService distributedLockService)
        {
            _dqtContext = dqtContext;
            _dataverseAdapter = dataverseAdapter;
            _currentClientProvider = currentClientProvider;
            _distributedLockService = distributedLockService;
        }

        public async Task<TrnRequestInfo> Handle(GetOrCreateTrnRequest request, CancellationToken cancellationToken)
        {
            var currentClientId = _currentClientProvider.GetCurrentClientId();

            await using var requestIdLock = await _distributedLockService.AcquireLock(key: $"{currentClientId}:{request.RequestId}", _lockTimeout);

            await using var husidLock = !string.IsNullOrEmpty(request.HusId) ?
                await _distributedLockService.AcquireLock(request.HusId, _lockTimeout) :
                NoopAsyncDisposable.Instance;

            var trnRequest = await _dqtContext.TrnRequests
                .SingleOrDefaultAsync(r => r.ClientId == currentClientId && r.RequestId == request.RequestId);

            bool wasCreated;
            string trn;

            if (trnRequest != null)
            {
                var teacher = trnRequest.TeacherId.HasValue ?
                    await _dataverseAdapter.GetTeacher(trnRequest.TeacherId.Value) :
                    null;

                wasCreated = false;
                trn = teacher?.dfeta_TRN;
            }
            else
            {
                var firstName = request.FirstName;
                var middleName = request.MiddleName ?? string.Empty;
                var lastName = request.LastName;

                var isHesaTrainee = !string.IsNullOrEmpty(request.HusId);
                if (isHesaTrainee)
                {
                    var firstAndMiddleNames = $"{firstName} {middleName}".Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    firstName = firstAndMiddleNames[0];
                    middleName = string.Join(" ", firstAndMiddleNames.Skip(1));
                }

                var createTeacherResult = await _dataverseAdapter.CreateTeacher(new CreateTeacherCommand()
                {
                    FirstName = firstName,
                    MiddleName = middleName,
                    LastName = lastName,
                    BirthDate = request.BirthDate.ToDateTime(),
                    EmailAddress = request.EmailAddress,
                    Address = new CreateTeacherCommandAddress()
                    {
                        AddressLine1 = request.Address?.AddressLine1,
                        AddressLine2 = request.Address?.AddressLine2,
                        AddressLine3 = request.Address?.AddressLine3,
                        City = request.Address?.City,
                        PostalCode = request.Address?.PostalCode,
                        Country = request.Address?.Country
                    },
                    GenderCode = request.GenderCode.ConvertToContact_GenderCode(),
                    InitialTeacherTraining = new CreateTeacherCommandInitialTeacherTraining()
                    {
                        ProviderUkprn = request.InitialTeacherTraining.ProviderUkprn,
                        ProgrammeStartDate = request.InitialTeacherTraining.ProgrammeStartDate.Value,
                        ProgrammeEndDate = request.InitialTeacherTraining.ProgrammeEndDate.Value,
                        ProgrammeType = request.InitialTeacherTraining.ProgrammeType.Value.ConvertToIttProgrammeType(),
                        Subject1 = request.InitialTeacherTraining.Subject1,
                        Subject2 = request.InitialTeacherTraining.Subject2,
                        Subject3 = request.InitialTeacherTraining.Subject3,
                        AgeRangeFrom = request.InitialTeacherTraining.AgeRangeFrom.HasValue ? AgeRange.ConvertFromValue(request.InitialTeacherTraining.AgeRangeFrom.Value) : null,
                        AgeRangeTo = request.InitialTeacherTraining.AgeRangeTo.HasValue ? AgeRange.ConvertFromValue(request.InitialTeacherTraining.AgeRangeTo.Value) : null,
                        IttQualificationValue = request.InitialTeacherTraining.IttQualificationType?.GetIttQualificationValue(),
                        IttQualificationAim = request.InitialTeacherTraining.IttQualificationAim?.ConvertToIttQualficationAim()
                    },
                    Qualification = request.Qualification != null ?
                        new CreateTeacherCommandQualification()
                        {
                            ProviderUkprn = request.Qualification.ProviderUkprn,
                            CountryCode = request.Qualification.CountryCode,
                            Subject = request.Qualification.Subject,
                            Class = request.Qualification.Class?.ConvertToClassDivision(),
                            Date = request.Qualification.Date.Value,
                            HeQualificationValue = request.Qualification.HeQualificationType?.GetHeQualificationValue(),
                            Subject2 = request.Qualification.Subject2,
                            Subject3 = request.Qualification.Subject3
                        } :
                        null,
                    HusId = request.HusId
                });

                if (!createTeacherResult.Succeeded)
                {
                    throw CreateValidationExceptionFromFailedReasons(createTeacherResult.FailedReasons);
                }

                _dqtContext.TrnRequests.Add(new TrnRequest()
                {
                    ClientId = currentClientId,
                    RequestId = request.RequestId,
                    TeacherId = createTeacherResult.TeacherId
                });

                await _dqtContext.SaveChangesAsync();

                wasCreated = true;
                trn = createTeacherResult.Trn;
            }

            var status = trn != null ? TrnRequestStatus.Completed : TrnRequestStatus.Pending;

            return new TrnRequestInfo()
            {
                WasCreated = wasCreated,
                RequestId = request.RequestId,
                Trn = trn,
                Status = status
            };
        }

        private ValidationException CreateValidationExceptionFromFailedReasons(CreateTeacherFailedReasons failedReasons)
        {
            var failures = new List<ValidationFailure>();

            ConsumeReason(
                CreateTeacherFailedReasons.IttProviderNotFound,
                $"{nameof(GetOrCreateTrnRequest.InitialTeacherTraining)}.{nameof(GetOrCreateTrnRequest.InitialTeacherTraining.ProviderUkprn)}",
                ErrorRegistry.OrganisationNotFound().Title);

            ConsumeReason(
                CreateTeacherFailedReasons.Subject1NotFound,
                $"{nameof(GetOrCreateTrnRequest.InitialTeacherTraining)}.{nameof(GetOrCreateTrnRequest.InitialTeacherTraining.Subject1)}",
                ErrorRegistry.SubjectNotFound().Title);

            ConsumeReason(
                CreateTeacherFailedReasons.Subject2NotFound,
                $"{nameof(GetOrCreateTrnRequest.InitialTeacherTraining)}.{nameof(GetOrCreateTrnRequest.InitialTeacherTraining.Subject2)}",
                ErrorRegistry.SubjectNotFound().Title);

            ConsumeReason(
                CreateTeacherFailedReasons.Subject3NotFound,
                $"{nameof(GetOrCreateTrnRequest.InitialTeacherTraining)}.{nameof(GetOrCreateTrnRequest.InitialTeacherTraining.Subject3)}",
                ErrorRegistry.SubjectNotFound().Title);

            ConsumeReason(
                CreateTeacherFailedReasons.IttQualificationNotFound,
                $"{nameof(GetOrCreateTrnRequest.InitialTeacherTraining)}.{nameof(GetOrCreateTrnRequest.InitialTeacherTraining.IttQualificationType)}",
                ErrorRegistry.IttQualificationNotFound().Title);

            ConsumeReason(
                CreateTeacherFailedReasons.QualificationCountryNotFound,
                $"{nameof(GetOrCreateTrnRequest.Qualification)}.{nameof(GetOrCreateTrnRequest.Qualification.CountryCode)}",
                ErrorRegistry.CountryNotFound().Title);

            ConsumeReason(
                CreateTeacherFailedReasons.QualificationSubjectNotFound,
                $"{nameof(GetOrCreateTrnRequest.Qualification)}.{nameof(GetOrCreateTrnRequest.Qualification.Subject)}",
                ErrorRegistry.SubjectNotFound().Title);

            ConsumeReason(
                CreateTeacherFailedReasons.QualificationSubject2NotFound,
                $"{nameof(GetOrCreateTrnRequest.Qualification)}.{nameof(GetOrCreateTrnRequest.Qualification.Subject2)}",
                ErrorRegistry.SubjectNotFound().Title);

            ConsumeReason(
                CreateTeacherFailedReasons.QualificationSubject3NotFound,
                $"{nameof(GetOrCreateTrnRequest.Qualification)}.{nameof(GetOrCreateTrnRequest.Qualification.Subject3)}",
                ErrorRegistry.SubjectNotFound().Title);

            ConsumeReason(
                CreateTeacherFailedReasons.QualificationProviderNotFound,
                $"{nameof(GetOrCreateTrnRequest.Qualification)}.{nameof(GetOrCreateTrnRequest.Qualification.ProviderUkprn)}",
                ErrorRegistry.OrganisationNotFound().Title);

            ConsumeReason(
                CreateTeacherFailedReasons.DuplicateHusId,
                $"{nameof(GetOrCreateTrnRequest.HusId)}.{nameof(GetOrCreateTrnRequest.HusId)}",
                ErrorRegistry.ExistingTeacherAlreadyHasHusId().Title);

            if (failedReasons != CreateTeacherFailedReasons.None)
            {
                throw new NotImplementedException($"Unknown {nameof(CreateTeacherFailedReasons)}: '{failedReasons}.");
            }

            return new ValidationException(failures);

            void ConsumeReason(CreateTeacherFailedReasons reason, string propertyName, string message)
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
