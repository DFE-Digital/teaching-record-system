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
        AddTeacherStatuses();
        AddEarlyYearsStatuses();

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

        _xrmFakedContext.CreateEntity(new dfeta_sanctioncode()
        {
            dfeta_Value = "B1",
            dfeta_name = "B1 Description"
        });
    }

    private void AddTeacherStatuses()
    {
        _xrmFakedContext.CreateEntity(new dfeta_teacherstatus()
        {
            dfeta_Value = "71",
            dfeta_name = "Qualified Teacher (trained)",
            dfeta_QTSDateRequired = true
        });

        _xrmFakedContext.CreateEntity(new dfeta_teacherstatus()
        {
            dfeta_Value = "100",
            dfeta_name = "Qualified Teacher: Assessment Only Route",
            dfeta_QTSDateRequired = true
        });

        _xrmFakedContext.CreateEntity(new dfeta_teacherstatus()
        {
            dfeta_Value = "90",
            dfeta_name = "Qualified teacher: by virtue of achieving international qualified teacher status",
            dfeta_QTSDateRequired = true
        });

        _xrmFakedContext.CreateEntity(new dfeta_teacherstatus()
        {
            dfeta_Value = "213",
            dfeta_name = "Qualified Teacher: QTS awarded in Wales",
            dfeta_QTSDateRequired = true
        });
    }

    private void AddEarlyYearsStatuses()
    {
        _xrmFakedContext.CreateEntity(new dfeta_earlyyearsstatus()
        {
            dfeta_Value = "220",
            dfeta_name = "Early Years Trainee",
        });

        _xrmFakedContext.CreateEntity(new dfeta_earlyyearsstatus()
        {
            dfeta_Value = "221",
            dfeta_name = "Early Years Teacher Status"
        });

        _xrmFakedContext.CreateEntity(new dfeta_earlyyearsstatus()
        {
            dfeta_Value = "222",
            dfeta_name = "Early Years Professional Status"
        });
    }
}
