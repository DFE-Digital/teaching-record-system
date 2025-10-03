using TUnit.Core.Interfaces;

namespace TeachingRecordSystem.TestCommon;

public record LimitToDbPoolSizeLimit : IParallelLimit
{
    public int Limit => 200;
}
