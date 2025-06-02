namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public Task<CreateIntegrationTransactionResult> CreateIntegrationTransactionAsync(Action<CreateIntegrationTransactionBuilder>? configure)
    {
        var builder = new CreateIntegrationTransactionBuilder();
        configure?.Invoke(builder);
        return builder.ExecuteAsync(this);
    }

    public class CreateIntegrationTransactionBuilder
    {
        public int? _totalCount;
        public int? _successCount;
        public int? _failureCount;
        public int? _duplicateCount;
        public string? _fileName;
        public IntegrationTransactionImportStatus? _importStatus;
        public IntegrationTransactionInterfaceType? _interfaceType;
        public DateTime? _createdOn;

        public CreateIntegrationTransactionBuilder WithTotalCount(int totalCount)
        {
            if (_totalCount is not null && _totalCount != totalCount)
            {
                throw new InvalidOperationException("WithTotalCount has already been set");
            }

            _totalCount = totalCount;
            return this;
        }

        public CreateIntegrationTransactionBuilder WithSuccesCount(int successCount)
        {
            if (_successCount is not null && _successCount != successCount)
            {
                throw new InvalidOperationException("WithSuccesCount has already been set");
            }

            _successCount = successCount;
            return this;
        }

        public CreateIntegrationTransactionBuilder WithFailureCount(int failureCount)
        {
            if (_failureCount is not null && _failureCount != failureCount)
            {
                throw new InvalidOperationException("WithFailureCount has already been set");
            }

            _failureCount = failureCount;
            return this;
        }

        public CreateIntegrationTransactionBuilder WithDuplicateCount(int duplicateCount)
        {
            if (_duplicateCount is not null && _duplicateCount != duplicateCount)
            {
                throw new InvalidOperationException("WithDuplicateCount has already been set");
            }

            _duplicateCount = duplicateCount;
            return this;
        }

        public CreateIntegrationTransactionBuilder WithFileName(string fileName)
        {
            if (_fileName is not null && _fileName != fileName)
            {
                throw new InvalidOperationException("WithFileName has already been set");
            }

            _fileName = fileName;
            return this;
        }

        public CreateIntegrationTransactionBuilder WithImportStatus(IntegrationTransactionImportStatus importStatus)
        {
            if (_importStatus is not null && _importStatus != importStatus)
            {
                throw new InvalidOperationException("WithImportStatus has already been set");
            }

            _importStatus = importStatus;
            return this;
        }

        public CreateIntegrationTransactionBuilder WithInterfaceType(IntegrationTransactionInterfaceType interfaceType)
        {
            if (_interfaceType is not null && _interfaceType != interfaceType)
            {
                throw new InvalidOperationException("WithFileName has already been set");
            }

            _interfaceType = interfaceType;
            return this;
        }

        public CreateIntegrationTransactionBuilder WithCreatedOn(DateTime createdOn)
        {
            if (_createdOn is not null && _createdOn != createdOn)
            {
                throw new InvalidOperationException("WithCreatedOn has already been set");
            }

            _createdOn = createdOn;
            return this;
        }


        public async Task<CreateIntegrationTransactionResult> ExecuteAsync(TestData testData)
        {
            Int64 integrationId = 0;

            await testData.WithDbContextAsync(async dbContext =>
            {
                var inegrationTransaction = new Core.DataStore.Postgres.Models.IntegrationTransaction()
                {
                    IntegrationTransactionId = 0,
                    CreatedDate = _createdOn!.Value,
                    TotalCount = _totalCount!.Value,
                    DuplicateCount = _duplicateCount!.Value,
                    SuccessCount = _successCount!.Value,
                    FailureCount = _failureCount!.Value,
                    FileName = _fileName!,
                    InterfaceType = _interfaceType!.Value,
                    ImportStatus = _importStatus!.Value,
                };
                dbContext.IntegrationTransactions.Add(inegrationTransaction);
                await dbContext.SaveChangesAsync();

                integrationId = inegrationTransaction.IntegrationTransactionId;

            });

            return new CreateIntegrationTransactionResult()
            {
                IntegrationTransactionId = integrationId

            };
        }
    }
}

public record CreateIntegrationTransactionResult
{
    public required long IntegrationTransactionId { get; init; }
}
