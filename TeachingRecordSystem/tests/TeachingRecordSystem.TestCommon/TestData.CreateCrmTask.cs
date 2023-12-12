using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public Task CreateCrmTask(Action<CreateCrmTaskBuilder>? configure)
    {
        var builder = new CreateCrmTaskBuilder();
        configure?.Invoke(builder);
        return builder.Execute(this);
    }

    public class CreateCrmTaskBuilder
    {
        private Guid? _personId = null;
        private string? _category = null;
        private string? _subject = null;
        private string? _description = null;
        private DateTime? _dueDate = null;
        private TaskState? _stateCode = null;

        public CreateCrmTaskBuilder WithPersonId(Guid personId)
        {
            if (_personId is not null && _personId != personId)
            {
                throw new InvalidOperationException("WithPersonId has already been set");
            }

            _personId = personId;
            return this;
        }

        public CreateCrmTaskBuilder WithCategory(string category)
        {
            if (_category is not null && _category != category)
            {
                throw new InvalidOperationException("WithCategory has already been set");
            }

            _category = category;
            return this;
        }

        public CreateCrmTaskBuilder WithSubject(string subject)
        {
            if (_subject is not null && _subject != subject)
            {
                throw new InvalidOperationException("WithSubject has already been set");
            }

            _subject = subject;
            return this;
        }

        public CreateCrmTaskBuilder WithDescription(string description)
        {
            if (_description is not null && _description != description)
            {
                throw new InvalidOperationException("WithDescription has already been set");
            }

            _description = description;
            return this;
        }

        public CreateCrmTaskBuilder WithDueDate(DateTime dueDate)
        {
            if (_dueDate is not null && _dueDate != dueDate)
            {
                throw new InvalidOperationException("WithDueDate has already been set");
            }

            _dueDate = dueDate;
            return this;
        }

        public CreateCrmTaskBuilder WithOpenStatus()
        {
            if (_stateCode is not null && _stateCode != TaskState.Open)
            {
                throw new InvalidOperationException("Task status cannot be changed after it's set.");
            }

            _stateCode = TaskState.Open;
            return this;
        }

        public CreateCrmTaskBuilder WithCompletedStatus()
        {
            if (_stateCode is not null && _stateCode != TaskState.Completed)
            {
                throw new InvalidOperationException("Task status cannot be changed after it's set.");
            }

            _stateCode = TaskState.Completed;
            return this;
        }

        public CreateCrmTaskBuilder WithCanceledStatus()
        {
            if (_stateCode is not null && _stateCode != TaskState.Canceled)
            {
                throw new InvalidOperationException("Task status cannot be changed after it's set.");
            }

            _stateCode = TaskState.Canceled;
            return this;
        }

        public async Task Execute(TestData crmTestData)
        {
            if (_personId is null)
            {
                throw new InvalidOperationException("WithPersonId has not been set");
            }

            var category = _category ?? "General";
            var subject = _subject ?? "Test Task";
            var description = _description ?? "Test Task Description";
            var stateCode = _stateCode ?? TaskState.Open;

            var task = new CrmTask()
            {
                Id = Guid.NewGuid(),
                RegardingObjectId = new EntityReference(Contact.EntityLogicalName, _personId.Value),
                Category = category,
                Subject = subject,
                Description = description,
                ScheduledEnd = _dueDate,
                StateCode = stateCode
            };

            var txnRequestBuilder = RequestBuilder.CreateTransaction(crmTestData.OrganizationService);
            txnRequestBuilder.AddRequest<CreateResponse>(new CreateRequest() { Target = task });

            if (stateCode == TaskState.Completed)
            {
                txnRequestBuilder.AddRequest<SetStateResponse>(new SetStateRequest()
                {
                    EntityMoniker = task.ToEntityReference(),
                    State = new OptionSetValue((int)TaskState.Completed),
                    Status = new OptionSetValue((int)Task_StatusCode.Completed)
                });
            }
            else if (stateCode == TaskState.Canceled)
            {
                txnRequestBuilder.AddRequest<SetStateResponse>(new SetStateRequest()
                {
                    EntityMoniker = task.ToEntityReference(),
                    State = new OptionSetValue((int)TaskState.Canceled),
                    Status = new OptionSetValue((int)Task_StatusCode.Canceled)
                });
            }

            await txnRequestBuilder.Execute();
        }
    }
}
