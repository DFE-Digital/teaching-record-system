using FakeXrmEasy.Abstractions;
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
        AddMqEstablishments();
        AddSpecialisms();
        AddHeQualifications();
        AddHeSubjects();

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

        _xrmFakedContext.CreateEntity(new dfeta_mqestablishment()
        {
            dfeta_Value = "150",
            dfeta_name = "Postgraduate Diploma in Deaf Education, University of Manchester, School of Psychological Sciences"
        });
    }

    private void AddSpecialisms()
    {
        foreach (var specialism in MandatoryQualificationSpecialismRegistry.GetAll(includeLegacy: true))
        {
            _xrmFakedContext.CreateEntity(new dfeta_specialism()
            {
                dfeta_Value = specialism.DqtValue,
                dfeta_name = specialism.Title,
                StateCode = !specialism.Legacy ? dfeta_specialismState.Active : dfeta_specialismState.Inactive
            });
        }
    }

    private void AddHeQualifications()
    {
        _xrmFakedContext.CreateEntity(new dfeta_hequalification()
        {
            dfeta_name = "BEd",
            dfeta_Value = "001"
        });

        _xrmFakedContext.CreateEntity(new dfeta_hequalification()
        {
            dfeta_name = "BEd (Hons)",
            dfeta_Value = "002"
        });

        _xrmFakedContext.CreateEntity(new dfeta_hequalification()
        {
            dfeta_name = "BSc/Education",
            dfeta_Value = "003"
        });

        _xrmFakedContext.CreateEntity(new dfeta_hequalification()
        {
            dfeta_name = "BSc Hons /Education",
            dfeta_Value = "004"
        });

        _xrmFakedContext.CreateEntity(new dfeta_hequalification()
        {
            dfeta_name = "BTech/Education",
            dfeta_Value = "005"
        });

        _xrmFakedContext.CreateEntity(new dfeta_hequalification()
        {
            dfeta_name = "BTech (Hons) /Education",
            dfeta_Value = "006"
        });

        _xrmFakedContext.CreateEntity(new dfeta_hequalification()
        {
            dfeta_name = "BA/Education",
            dfeta_Value = "007"
        });

        _xrmFakedContext.CreateEntity(new dfeta_hequalification()
        {
            dfeta_name = "BA (Hons) /Education",
            dfeta_Value = "008"
        });
    }

    private void AddHeSubjects()
    {
        _xrmFakedContext.CreateEntity(new dfeta_hesubject()
        {
            dfeta_name = "Mathematics",
            dfeta_Value = "001"
        });

        _xrmFakedContext.CreateEntity(new dfeta_hesubject()
        {
            dfeta_name = "English",
            dfeta_Value = "002"
        });

        _xrmFakedContext.CreateEntity(new dfeta_hesubject()
        {
            dfeta_name = "Science",
            dfeta_Value = "003"
        });

        _xrmFakedContext.CreateEntity(new dfeta_hesubject()
        {
            dfeta_name = "Art",
            dfeta_Value = "004"
        });

        _xrmFakedContext.CreateEntity(new dfeta_hesubject()
        {
            dfeta_name = "Physical Education",
            dfeta_Value = "005"
        });

        _xrmFakedContext.CreateEntity(new dfeta_hesubject()
        {
            dfeta_name = "Geography",
            dfeta_Value = "006"
        });

        _xrmFakedContext.CreateEntity(new dfeta_hesubject()
        {
            dfeta_name = "History",
            dfeta_Value = "007"
        });
    }
}
