using System;
using System.Threading;
using System.Threading.Tasks;
using DqtApi.DataStore.Crm;
using DqtApi.DataStore.Sql;
using DqtApi.DataStore.Sql.Models;
using DqtApi.Security;
using DqtApi.V2.ApiModels;
using DqtApi.V2.Requests;
using DqtApi.V2.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DqtApi.V2.Handlers
{
    public class GetOrCreateTrnRequestHandler : IRequestHandler<GetOrCreateTrnRequest, TrnRequestInfo>
    {
        private readonly DqtContext _dqtContext;
        private readonly IDataverseAdapter _dataverseAdapter;
        private readonly ICurrentClientProvider _currentClientProvider;

        public GetOrCreateTrnRequestHandler(
            DqtContext dqtContext,
            IDataverseAdapter dataverseAdapter,
            ICurrentClientProvider currentClientProvider)
        {
            _dqtContext = dqtContext;
            _dataverseAdapter = dataverseAdapter;
            _currentClientProvider = currentClientProvider;
        }

        public async Task<TrnRequestInfo> Handle(GetOrCreateTrnRequest request, CancellationToken cancellationToken)
        {
            var currentClientId = _currentClientProvider.GetCurrentClientId();

            using var transaction = await _dqtContext.Database.BeginTransactionAsync();

            // Get an advisory lock, scoped to this client + request ID.
            // This prevents us racing with another request with the same IDs; we don't want multiple CRM records created.
            await transaction.AcquireAdvisoryLock(currentClientId, request.RequestId);

            var trnRequest = await _dqtContext.TrnRequests
                .SingleOrDefaultAsync(r => r.ClientId == currentClientId && r.RequestId == request.RequestId);

            bool wasCreated;
            string trn;

            if (trnRequest != null)
            {
                var teacher = trnRequest.TeacherId.HasValue ?
                    await _dataverseAdapter.GetTeacherAsync(trnRequest.TeacherId.Value) :
                    null;

                wasCreated = false;
                trn = teacher?.dfeta_TRN;
            }
            else
            {
                var createTeacherResult = await _dataverseAdapter.CreateTeacher(new CreateTeacherCommand()
                {
                    FirstName = request.FirstName,
                    MiddleName = request.MiddleName,
                    LastName = request.LastName,
                    BirthDate = request.BirthDate.ToDateTime(new()),
                    EmailAddress = request.EmailAddress,
                    Address = new CreateTeacherCommandAddress()
                    {
                        AddressLine1 = request.Address.AddressLine1,
                        AddressLine2 = request.Address.AddressLine2,
                        AddressLine3 = request.Address.AddressLine3,
                        City = request.Address.City,
                        PostalCode = request.Address.PostalCode,
                        Country = request.Address.Country
                    },
                    GenderCode = request.GenderCode.ConvertToContact_GenderCode(),
                    InitialTeacherTraining = new CreateTeacherCommandInitialTeacherTraining()
                    {
                        ProviderUkprn = request.InitialTeacherTraining.ProviderUkprn,
                        ProgrammeStartDate = request.InitialTeacherTraining.ProgrammeStartDate,
                        ProgrammeEndDate = request.InitialTeacherTraining.ProgrammeEndDate,
                        ProgrammeType = request.InitialTeacherTraining.ProgrammeType.ConvertToIttProgrammeType(),
                        Subject1 = request.InitialTeacherTraining.Subject1,
                        Subject2 = request.InitialTeacherTraining.Subject2,
                        Result = request.InitialTeacherTraining.Result.ConvertToITTResult()
                    },
                    Qualification = new CreateTeacherCommandQualification()
                    {
                        ProviderUkprn = request.Qualification.ProviderUkprn,
                        CountryCode = request.Qualification.CountryCode,
                        Subject = request.Qualification.Subject,
                        Class = request.Qualification.Class.ConvertToClassDivision(),
                        Date = request.Qualification.Date
                    }
                });

                if (!createTeacherResult.Succeeded)
                {
                    throw new NotImplementedException();
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

            await transaction.CommitAsync();

            var status = trn != null ? TrnRequestStatus.Completed : TrnRequestStatus.Pending;

            return new TrnRequestInfo()
            {
                WasCreated = wasCreated,
                RequestId = request.RequestId,
                Trn = trn,
                Status = status
            };
        }
    }
}
