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

    public Task ExecuteAsync()
    {
        AddSubjects();
        AddSanctionCodes();
        AddTeacherStatuses();
        AddEarlyYearsStatuses();
        AddMqEstablishments();
        AddSpecialisms();
        AddHeQualifications();
        AddHeSubjects();
        AddCountries();
        AddITTSubjects();
        AddITTQualifications();
        AddITTProviders();

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
        var sanctionCodesAndNames = new (string SanctionCode, string Name)[]
        {
            ("A10", "No Sanction  -  unacceptable professional conduct"),
            ("A11", "No Sanction  -  serious professional incompetence"),
            ("A12", "Suspension order  -  unacceptable professional conduct  -  with conditions"),
            ("A13", "Suspension order  -  serious professional incompetence  -  with conditions"),
            ("A14", "For internal information only - historic GTC finding of unsuitable for registration"),
            ("A15", "No Sanction  -  conviction for a relevant offence"),
            ("A16", "Reprimand  -  conviction of a relevant offence"),
            ("A17", "Conditional Registration Order  -  conviction of a relevant offence"),
            ("A18", "Suspension order  -  conviction of a relevant offence  -  without conditions"),
            ("A19", "Prohibition Order  -  unacceptable professional conduct  -  Ineligible to reapply"),
            ("A1A", "Prohibition Order  -  unacceptable professional conduct  -  Eligible to reapply after specified time"),
            ("A1B", "Suspension order  -  unacceptable professional conduct  -  without conditions"),
            ("A2", "Suspension order  -  conviction of a relevant offence  -  with conditions"),
            ("A20", "Prohibition Order  -  conviction of a relevant offence  -  ineligible to reapply"),
            ("A21A", "Prohibition Order  -  conviction of a relevant offence  -  eligible to reapply after specified time"),
            ("A21B", "No Sanction  -  breach of condition(s)"),
            ("A22", "Suspension order  -  without conditions  -  (arising from breach of previous condition(s))"),
            ("A23", "Suspension order  -  with conditions  -  (arising from breach of previous condition(s))"),
            ("A24", "Prohibition order  -  breach of condition(s)  -  ineligible to reapply"),
            ("A25A", "Prohibition Order  -  breach of condition(s)  -  eligible to reapply after specified time"),
            ("A25B", "Refer to Professional Standards  -  discontinued hearing"),
            ("A26", "Conditional Registration Order  -  unacceptable professional conduct"),
            ("A3", "Reprimand  -  unacceptable professional conduct"),
            ("A4", "Prohibition Order  -  serious professional incompetence  -  Ineligible to reapply"),
            ("A5A", "Prohibition Order  -  serious professional incompetence  -  Eligible to reapply after specified time"),
            ("A5B", "Suspension order  -  serious professional incompetence  -  without conditions"),
            ("A6", "Conditional Registration Order  -  serious professional incompetence"),
            ("A7", "Reprimand  -  serious professional incompetence"),
            ("A8", "Refer to Professional Standards Team"),
            ("A9", "Barring by the Secretary of State"),
            ("B1", "Restricted by the Secretary of State  -  Permitted to work as teacher"),
            ("B2A", "Restricted by the Secretary of State  -  Not Permitted to work as teacher"),
            ("B2B", "Prohibited by an Independent Schools Tribunal or Secretary of State"),
            ("B3", "Employers to contact the Secretary of State"),
            ("B4", "Prohibited in Scotland or Northern Ireland"),
            ("B5", "Formerly on List 99"),
            ("B6", "Prohibited by the Secretary of State  -  failed probation"),
            ("C1", "Failed induction"),
            ("C2", "Restricted by the Secretary of State  -  failed probation  -  permitted to carry out specified work for a period equal in length to a statutory induction period only"),
            ("C3", "Ineligible for registration  -  refer to General Teaching Council for Scotland"),
            ("D1", "Eligible for registration subject to General Teaching Council for Scotland disciplinary order"),
            ("D2", "Ineligible for registration subject to General Teaching Council for Wales disciplinary order"),
            ("D3", "Eligible for registration subject to General Teaching Council for Wales disciplinary order"),
            ("D4", "Ineligible for registration  -  refer to General Teaching Council for Northern Ireland"),
            ("D5", "Eligible for registration subject to General Teaching Council for Northern Ireland disciplinary order"),
            ("D6", "Ineligible for registration subject to Independent Schools Tribunal disciplinary drder"),
            ("D7", "Restricted  -  Deferred for skills test"),
            ("E1", "Restricted by statutory regulations"),
            ("E2", "For internal information only - record potentially included in historic GTC data loss"),
            ("E3", "For internal information only - historic GTC vexatious complainant"),
            ("E4", "Non Payment of Fee"),
            ("F1", "A possible matching record was found. Please contact the DBS before employing this person"),
            ("G1", "Formerly barred by the Independent Safeguarding Authority"),
            ("G2", "Prohibition by the Secretary of State - misconduct"),
            ("T1", "FOR INTERNAL INFORMATION ONLY - known duplicate record"),
            ("T10", "Interim prohibition by the Secretary of State"),
            ("T2", "Prohibition by the Secretary of State  -  deregistered by GTC Scotland"),
            ("T3", "Prohibition by the Secretary of State  -  refer to the Education Workforce Council, Wales"),
            ("T4", "Prohibition by the Secretary of State  -  refer to GTC Northern Ireland"),
            ("T5", "Secretary of State decision- no prohibition"),
            ("T6", "Section 128 barring direction"),
            ("T7", "Teacher sanctioned in other EEA member state"),
            ("T8", "FOR INTERNAL INFORMATION ONLY - see alert details"),
            ("T9", "Council Members never de-register"),
            ("Z1", "Council Member  -  do not register"),
            ("Z2", "")
        };

        Array.ForEach(sanctionCodesAndNames, s => _xrmFakedContext.CreateEntity(new dfeta_sanctioncode()
        {
            dfeta_name = s.Name,
            dfeta_Value = s.SanctionCode
        }));
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
            dfeta_name = "Early Years Trainee"
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

    private void AddCountries()
    {
        _xrmFakedContext.CreateEntity(new dfeta_country()
        {
            dfeta_name = "United Kingdom",
            dfeta_Value = "XK"
        });

        _xrmFakedContext.CreateEntity(new dfeta_country()
        {
            dfeta_name = "United Kingdom",
            dfeta_Value = "GB"
        });

        _xrmFakedContext.CreateEntity(new dfeta_country()
        {
            dfeta_name = "Wales",
            dfeta_Value = "WA"
        });

        _xrmFakedContext.CreateEntity(new dfeta_country()
        {
            dfeta_name = "Northern Ireland",
            dfeta_Value = "XG"
        });

        _xrmFakedContext.CreateEntity(new dfeta_country()
        {
            dfeta_name = "Scotland",
            dfeta_Value = "XH"
        });

        _xrmFakedContext.CreateEntity(new dfeta_country()
        {
            dfeta_name = "Portugal",
            dfeta_Value = "PT"
        });

        _xrmFakedContext.CreateEntity(new dfeta_country()
        {
            dfeta_name = "Spain",
            dfeta_Value = "ES"
        });

        _xrmFakedContext.CreateEntity(new dfeta_country()
        {
            dfeta_name = "France",
            dfeta_Value = "FR"
        });
    }

    private void AddITTSubjects()
    {
        _xrmFakedContext.CreateEntity(new dfeta_ittsubject()
        {
            dfeta_name = "business and management",
            dfeta_Value = "100078"
        });

        _xrmFakedContext.CreateEntity(new dfeta_ittsubject()
        {
            dfeta_name = "business studies",
            dfeta_Value = "100079"
        });

        _xrmFakedContext.CreateEntity(new dfeta_ittsubject()
        {
            dfeta_name = "applied biology",
            dfeta_Value = "100343"
        });

        _xrmFakedContext.CreateEntity(new dfeta_ittsubject()
        {
            dfeta_name = "classical studies",
            dfeta_Value = "100300"
        });
    }

    private void AddITTQualifications()
    {
        _xrmFakedContext.CreateEntity(new dfeta_ittqualification()
        {
            dfeta_name = "BA (Hons)",
            dfeta_Value = "008"
        });

        _xrmFakedContext.CreateEntity(new dfeta_ittqualification()
        {
            dfeta_name = "BSc",
            dfeta_Value = "003"
        });

        _xrmFakedContext.CreateEntity(new dfeta_ittqualification()
        {
            dfeta_name = "BTech/Education",
            dfeta_Value = "005"
        });

        _xrmFakedContext.CreateEntity(new dfeta_ittqualification()
        {
            dfeta_name = "Degree",
            dfeta_Value = "400"
        });
    }

    private void AddITTProviders()
    {
        _xrmFakedContext.CreateEntity(new Account()
        {
            Name = "ARK Teacher Training",
            dfeta_TrainingProvider = true,
            dfeta_UKPRN = "10044534"
        });

        _xrmFakedContext.CreateEntity(new Account()
        {
            Name = "University of Newcastle Upon Tyne",
            dfeta_TrainingProvider = true,
            dfeta_UKPRN = "10007799"
        });

        _xrmFakedContext.CreateEntity(new Account()
        {
            Name = "Non-UK establishment",
            dfeta_TrainingProvider = true
        });

        _xrmFakedContext.CreateEntity(new Account()
        {
            Name = "UK establishment (Scotland/Northern Ireland)",
            dfeta_TrainingProvider = true
        });
    }
}
