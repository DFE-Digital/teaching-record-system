using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DqtApi.DataStore.Crm;
using DqtApi.DataStore.Crm.Models;
using DqtApi.DataStore.Sql;
using DqtApi.V2.Requests;
using DqtApi.V2.Responses;
using DqtApi.Validation;
using MediatR;

namespace DqtApi.V2.Handlers
{
    public class SetTeacherIdentityInfoHandler : IRequestHandler<SetTeacherIdentityInfoRequest, TeacherIdentityInfo>
    {
        private readonly IDataverseAdapter _dataverseAdapter;
        private readonly DqtContext _dqtContext;

        public SetTeacherIdentityInfoHandler(IDataverseAdapter dataverseAdapter, DqtContext dqtContext)
        {
            _dataverseAdapter = dataverseAdapter;
            _dqtContext = dqtContext;
        }

        public async Task<TeacherIdentityInfo> Handle(SetTeacherIdentityInfoRequest request, CancellationToken cancellationToken)
        {
            using var transaction = await _dqtContext.Database.BeginTransactionAsync();

            // Acquire an advisory lock, scoped to the TsPersonId.
            // This prevents multiple requests trying to assign the given TsPersonId at the same time;
            // we need to ensure at most one record in DQT has a given TsPersonId.
            await transaction.AcquireAdvisoryLock("SetTsPersonId", request.TsPersonId);

            var teachersWithTrn = await _dataverseAdapter.GetTeachersByTrn(request.Trn, columnNames: new[] { Contact.Fields.dfeta_TSPersonID });

            if (teachersWithTrn.Length == 0)
            {
                throw new ErrorException(ErrorRegistry.TeacherWithSpecifiedTrnNotFound());
            }
            else if (teachersWithTrn.Length > 1)
            {
                throw new ErrorException(ErrorRegistry.MultipleTeachersFoundWithSpecifiedTrn());
            }

            var teacher = teachersWithTrn.Single();

            if (!string.IsNullOrEmpty(teacher.dfeta_TSPersonID) && teacher.dfeta_TSPersonID != request.TsPersonId)
            {
                throw new ErrorException(ErrorRegistry.TeacherAlreadyHasTsPersonIdDefined());
            }

            if (teacher.dfeta_TSPersonID != request.TsPersonId)
            {
                var existingTeacherWithTsPersonId = await _dataverseAdapter.GetTeacherByTsPersonId(request.TsPersonId);
                if (existingTeacherWithTsPersonId != null)
                {
                    throw new ErrorException(ErrorRegistry.AnotherTeacherHasTheSpecifiedTsPersonIdAssigned());
                }

                await _dataverseAdapter.SetTsPersonId(teacher.Id, request.TsPersonId);
            }

            return new TeacherIdentityInfo()
            {
                Trn = request.Trn,
                TsPersonId = request.TsPersonId
            };
        }
    }
}
