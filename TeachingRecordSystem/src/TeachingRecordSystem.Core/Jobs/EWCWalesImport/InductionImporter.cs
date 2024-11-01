using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.Core.Jobs.EWCWalesImport;
public class InductionImporter(ICrmQueryDispatcher crmQueryDispatcher)
{
    public Task Import(StreamReader csvReaderStream, Guid IntegrationTransactionId)
    {
        return Task.CompletedTask;
    }
}
