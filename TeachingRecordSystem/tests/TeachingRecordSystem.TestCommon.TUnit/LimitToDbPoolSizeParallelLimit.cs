using TUnit.Core.Interfaces;

namespace TeachingRecordSystem.TestCommon;

public record LimitToDbPoolSizeParallelLimit : IParallelLimit
{
    public int Limit => 100;
}
