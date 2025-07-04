using TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class CreateIntegrationTransactionRecordBuilder
{
    private string? _rowData;
    private string? _failureMessage;
    private Guid _personId;
    private bool? _duplicate;
    private DateTime _createdDate;
    private IntegrationTransactionRecordStatus _status;
    private bool _hasActiveAlerts;

    public CreateIntegrationTransactionRecordBuilder WithRowData(string rowData)
    {
        _rowData = rowData;
        return this;
    }

    public CreateIntegrationTransactionRecordBuilder WithFailureMessage(string failureMessage)
    {
        _failureMessage = failureMessage;
        return this;
    }

    public CreateIntegrationTransactionRecordBuilder WithPersonId(Guid personId)
    {
        _personId = personId;
        return this;
    }

    public CreateIntegrationTransactionRecordBuilder WithDuplicate(bool duplicate)
    {
        _duplicate = duplicate;
        return this;
    }

    public CreateIntegrationTransactionRecordBuilder WithCreatedDate(DateTime createdDate)
    {
        _createdDate = createdDate;
        return this;
    }

    public CreateIntegrationTransactionRecordBuilder WithStatus(IntegrationTransactionRecordStatus status)
    {
        _status = status;
        return this;
    }

    public CreateIntegrationTransactionRecordBuilder WithActiveAlerts(bool hasActiveAlerts)
    {
        _hasActiveAlerts = hasActiveAlerts;
        return this;
    }

    public IntegrationTransactionRecord Execute()
    {
        return new IntegrationTransactionRecord
        {
            IntegrationTransactionRecordId = 0,
            RowData = _rowData,
            FailureMessage = _failureMessage,
            PersonId = _personId,
            Duplicate = _duplicate,
            CreatedDate = _createdDate,
            Status = _status,
            HasActiveAlert = _hasActiveAlerts
        };
    }
}
