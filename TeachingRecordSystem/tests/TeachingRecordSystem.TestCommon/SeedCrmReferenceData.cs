using FakeXrmEasy.Abstractions;
using TeachingRecordSystem.Core;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Models;

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
        AddMqEstablishments();
        AddSpecialisms();

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

        _xrmFakedContext.CreateEntity(new dfeta_teacherstatus()
        {
            dfeta_Value = "212",
            dfeta_name = "Assessment only route candidate",
            dfeta_QTSDateRequired = false
        });

        _xrmFakedContext.CreateEntity(new dfeta_teacherstatus()
        {
            dfeta_Value = "214",
            dfeta_name = "Partial qualified teacher status",
            dfeta_QTSDateRequired = false
        });

        _xrmFakedContext.CreateEntity(new dfeta_teacherstatus()
        {
            dfeta_Value = "211",
            dfeta_name = "Trainee teacher",
            dfeta_QTSDateRequired = true
        });

        _xrmFakedContext.CreateEntity(new dfeta_teacherstatus()
        {
            dfeta_Value = "103",
            dfeta_name = "Qualified Teacher: By virtue of overseas qualifications",
            dfeta_QTSDateRequired = true
        });

        _xrmFakedContext.CreateEntity(new dfeta_teacherstatus()
        {
            dfeta_Value = "104",
            dfeta_name = "Qualified Teacher (by virtue of non-UK teaching qualifications)",
            dfeta_QTSDateRequired = true
        });

        _xrmFakedContext.CreateEntity(new dfeta_teacherstatus()
        {
            dfeta_Value = "206",
            dfeta_name = "Qualified Teacher: Under temporary provision 2005/36/EC",
            dfeta_QTSDateRequired = true
        });

        _xrmFakedContext.CreateEntity(new dfeta_teacherstatus()
        {
            dfeta_Value = "223",
            dfeta_name = "Qualified Teacher (by virtue of European teaching qualifications)",
            dfeta_QTSDateRequired = true
        });

        _xrmFakedContext.CreateEntity(new dfeta_teacherstatus()
        {
            dfeta_Value = "50",
            dfeta_name = "Person not qualified for employment as a regular teacher whose employment as an Instructor is allowed under Regulation 18 of the Schools Regulations 1959",
            dfeta_QTSDateRequired = false
        });

        _xrmFakedContext.CreateEntity(new dfeta_teacherstatus()
        {
            dfeta_Value = "67",
            dfeta_name = "Qualified Teacher: Under the EC Directive",
            dfeta_QTSDateRequired = true
        });

        _xrmFakedContext.CreateEntity(new dfeta_teacherstatus()
        {
            dfeta_Value = "68",
            dfeta_name = "Qualified Teacher: Teachers trained/registered in Scotland",
            dfeta_QTSDateRequired = true
        });

        _xrmFakedContext.CreateEntity(new dfeta_teacherstatus()
        {
            dfeta_Value = "69",
            dfeta_name = "Qualified Teacher: Teachers trained/registered in Scotland",
            dfeta_QTSDateRequired = true
        });

        _xrmFakedContext.CreateEntity(new dfeta_teacherstatus()
        {
            dfeta_Value = "87",
            dfeta_name = "Qualified teacher (under the EC Directive) further qualified to teach the deaf or partially hearing under Regulation 15(2) of the Special Schools and Handicapped Pupils Regulations 1959",
            dfeta_QTSDateRequired = true
        });

        _xrmFakedContext.CreateEntity(new dfeta_teacherstatus()
        {
            dfeta_Value = "28",
            dfeta_name = "Qualified Teacher: Teacher trained/registered in Scotland, further qualified to teach the deaf or partially hearing impaired under Regulation 15(2) of the Special Schools and Handicapped Pupils Regulations 1959.",
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

    private void AddMqEstablishments()
    {
        _xrmFakedContext.CreateEntity(new dfeta_mqestablishment()
        {
            dfeta_Value = "955",
            dfeta_name = "University of Birmingham"
        });

        _xrmFakedContext.CreateEntity(new dfeta_mqestablishment()
        {
            dfeta_Value = "210",
            dfeta_name = "Postgraduate Diploma in Multi-Sensory Impairment and Deafblindness, University of Birmingham, School of Education"
        });

        _xrmFakedContext.CreateEntity(new dfeta_mqestablishment()
        {
            dfeta_Value = "957",
            dfeta_name = "University of Edinburgh"
        });

        _xrmFakedContext.CreateEntity(new dfeta_mqestablishment()
        {
            dfeta_Value = "959",
            dfeta_name = "University of Leeds"
        });

        _xrmFakedContext.CreateEntity(new dfeta_mqestablishment()
        {
            dfeta_Value = "961",
            dfeta_name = "University of Manchester"
        });
    }

    private void AddSpecialisms()
    {
        foreach (var specialism in MandatoryQualificationSpecialismRegistry.GetAll())
        {
            _xrmFakedContext.CreateEntity(new dfeta_specialism()
            {
                dfeta_Value = specialism.DqtValue,
                dfeta_name = specialism.Title,
                StateCode = specialism.IsValidForNewRecord ? dfeta_specialismState.Active : dfeta_specialismState.Inactive
            });
        }
    }
}
