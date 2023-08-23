using FakeXrmEasy.Abstractions;
using Microsoft.Xrm.Sdk;
using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.TestCommon;

public static class XrmFakedContextExtensions
{
    public static void DeleteAllEntities<TEntity>(this IXrmFakedContext xrmFakedContext)
        where TEntity : Entity
    {
        var allEntities = xrmFakedContext.CreateQuery<TEntity>().ToList();

        foreach (var entity in allEntities)
        {
            xrmFakedContext.DeleteEntity(entity.Id.ToEntityReference(entity.LogicalName));
        }
    }
}
