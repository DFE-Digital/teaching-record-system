using FakeXrmEasy.Abstractions;
using TeachingRecordSystem.Core;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.TestCommon;

public class SeedCrmReferenceData : IStartupTask
{
    private readonly IXrmFakedContext _xrmFakedContext;

    public SeedCrmReferenceData(IXrmFakedContext xrmFakedContext)
    {
        _xrmFakedContext = xrmFakedContext;
    }

    public Task Execute()
    {
        AddSubjects();
        AddSanctionCodes();

        return Task.CompletedTask;
    }

    private void AddSubjects()
    {
        _xrmFakedContext.CreateEntity(new Subject()
        {
            Title = "Change of Date of Birth"
        });

        _xrmFakedContext.CreateEntity(new Subject()
        {
            Title = "Change of Name"
        });
    }

    private void AddSanctionCodes()
    {
        _xrmFakedContext.CreateEntity(new dfeta_sanctioncode()
        {
            dfeta_Value = "G1",
            dfeta_name = "G1 Description"
        });

        _xrmFakedContext.CreateEntity(new dfeta_sanctioncode()
        {
            dfeta_Value = "A1",
            dfeta_name = "A1 Description"
        });

        _xrmFakedContext.CreateEntity(new dfeta_sanctioncode()
        {
            dfeta_Value = "A17",
            dfeta_name = "A17 Description"
        });

        _xrmFakedContext.CreateEntity(new dfeta_sanctioncode()
        {
            dfeta_Value = "A18",
            dfeta_name = "A18 Description"
        });
    }
}
