namespace TeachingRecordSystem.Core;

public class LegacyDataCache
{
    private LegacyDataCache() { }

    public static LegacyDataCache Instance { get; } = new();

    public IReadOnlyCollection<MqEstablishment> MqEstablishments { get; } =
    [
        new("Liverpool John Moores University", "964",  true),
        new("Plymouth University",  "965",  true),
        new("Diploma in Professional Studies in Education for Teachers of Visually Handicapped Children - Moray House College of Education, Edinburgh", "10", false),
        new("Diploma for Teachers of the Deaf - University College, Dublin", "100", false),
        new("Diploma in Education of Deaf and Partially Hearing Children - Moray House College of Education, Edinburgh", "110", false),
        new("Postgraduate Diploma in Education (Special Education: Hearing Impairment), University of Birmingham, School of Education", "120", false),
        new("Diploma in Special Education (Hearing Impaired) - University of Swansea",  "130", false),
        new("Postgraduate Diploma (Education of Deaf Children), University of Hertfordshire", "140", false),
        new("Postgraduate Diploma in Deaf Education, University of Manchester, School of Psychological Sciences", "150", false),
        new("BPhil in Education (Special Education: Hearing Impairment), University of Birmingham, School of Education", "160", false),
        new("Diploma in Special Educational Needs: Hearing Impaired - Gwent College", "170", false),
        new("BPhil in Multi-Sensory Impairment and Deafblindness, University of Birmingham, School of Education", "180", false),
        new("Diploma in the Education and Psychology of Children with Special Needs - Multi-Sensory Impairment (Deaf - Blind) - University of London, Institute of Education in conjunction with Whitfield School, Walthamstow", "190", false),
        new("BPhil for Teachers of Children with a Visual Impairment, University of Birmingham, School of Education", "20", false),
        new("Diploma in the Education and Psychology of Children with Special Needs - Multi", "200", false),
        new("Postgraduate Diploma in Multi-Sensory Impairment and Deafblindness, University of Birmingham, School of Education", "210", false),
        new("MA in Deaf Education (Teacher of the Deaf Qualification), University of Leeds, School of Education", "220", false),
        new("Postgraduate Diploma in Education Studies (Hearing Impairment), Oxford Brookes University, Westminster Institute of Education, in partnership with Mary Hare School",  "230", false),
        new("Postgraduate Diploma (Teachers Working with Children with Multi-Sensory Impairment), Kingston University, School of Education, in partnership with Whitefield Schools and Centre", "240", false),
        new("Postgraduate Diploma for Teachers of Children with Visual Impairment, University of Birmingham, School of Education", "30", false),
        new("Diploma in the Education of Children with Disabilities of Sight - Cambridge Institute of Education", "40", false),
        new("Graduate Diploma in Special and Inclusive Education: Disabilities of Sight, University of London Institute of Education", "50", false),
        new("Diploma in Professional Practice in Education (SEN) in Visual Impairment - Manchester Metropolitan University", "60", false),
        new("Diploma in Special Education (Visually Impaired) - University of Swansea", "70", false),
        new("Diploma in Special Educational Needs: Visually Impaired - Gwent College", "80", false),
        new("Masters Level: Mandatory Qualification for Teachers of Children with Visual Impairment, University of Plymouth, Faculty of Education, in partnership with the Sensory Consortium", "90", false),
        new("Special courses of advanced study and courses leading to higher degrees and degrees in education other than BEd degrees", "950", false),
        new("Bristol Polytechnic", "951", false),
        new("Courses for teaching handicapped children other than the blind, deaf and partially hearing", "952", false),
        new("One-year courses (originally known as Supplementary Courses)", "953", false),
        new("University College, Swansea", "954", true),
        new("University of Birmingham", "955", true),
        new("University of Cambridge", "956", true),
        new("University of Edinburgh", "957", true),
        new("University of Hertfordshire", "958", true),
        new("University of Leeds", "959", true),
        new("University of London", "960", true),
        new("University of Manchester", "961", true),
        new("University of Newcastle-upon-Tyne", "962", true),
        new("University of Oxford/Oxford Polytechnic", "963", true)
    ];

    public IReadOnlyCollection<MqSpecialism> MqSpecialisms { get; } =
    [
        new("Deaf education", "Deaf education", true),
        new("N/A", "N/A", true),
        new("Auditory Impairment", "Auditory", false),
        new("Hearing", "Hearing", true),
        new("Multi_Sensory Impairment", "Multi-Sensory", true),
        new("Visual Impairment", "Visual", true)
    ];

    public IReadOnlyCollection<SanctionCode> SanctionCodes { get; } =
    [
        new("FOR INTERNAL INFORMATION ONLY - known duplicate record", "T10", true),
        new("FOR INTERNAL INFORMATION ONLY - see alert details", "T9", true),
        new("Section 128 barring direction", "T7", true),
        new("Teacher sanctioned in other EEA member state", "T8", true),
        new("Refer to Registration", "A10", false),
        new("No Sanction  -  unacceptable professional conduct", "A11", true),
        new("No Sanction  -  serious professional incompetence", "A12", false),
        new("Suspension order  -  unacceptable professional conduct  -  with conditions", "A13", false),
        new("Suspension order  -  serious professional incompetence  -  with conditions", "A14", false),
        new("For internal information only - historic GTC finding of unsuitable for registration", "A15", false),
        new("No Sanction  -  conviction for a relevant offence", "A16", true),
        new("Reprimand  -  conviction of a relevant offence", "A17", false),
        new("Conditional Registration Order  -  conviction of a relevant offence", "A18", false),
        new("Suspension order  -  conviction of a relevant offence  -  without conditions", "A19", false),
        new("Prohibition Order  -  unacceptable professional conduct  -  Ineligible to reapply", "A1A", false),
        new("Prohibition Order  -  unacceptable professional conduct  -  Eligible to reapply after specified time", "A1B", false),
        new("Suspension order  -  unacceptable professional conduct  -  without conditions", "A2", false),
        new("Suspension order  -  conviction of a relevant offence  -  with conditions", "A20", false),
        new("Prohibition Order  -  conviction of a relevant offence  -  ineligible to reapply", "A21A", false),
        new("Prohibition Order  -  conviction of a relevant offence  -  eligible to reapply after specified time", "A21B", false),
        new("No Sanction  -  breach of condition(s)", "A22", false),
        new("Suspension order  -  without conditions  -  (arising from breach of previous condition(s))", "A23", false),
        new("Suspension order  -  with conditions  -  (arising from breach of previous condition(s))", "A24", false),
        new("Prohibition order  -  breach of condition(s)  -  ineligible to reapply", "A25A", false),
        new("Prohibition Order  -  breach of condition(s)  -  eligible to reapply after specified time", "A25B", false),
        new("Refer to Professional Standards  -  discontinued hearing", "A26", false),
        new("Conditional Registration Order  -  unacceptable professional conduct", "A3", false),
        new("Reprimand  -  unacceptable professional conduct", "A4", false),
        new("Prohibition Order  -  serious professional incompetence  -  Ineligible to reapply", "A5A", false),
        new("Prohibition Order  -  serious professional incompetence  -  Eligible to reapply after specified time", "A5B", false),
        new("Suspension order  -  serious professional incompetence  -  without conditions", "A6", false),
        new("Conditional Registration Order  -  serious professional incompetence", "A7", false),
        new("Reprimand  -  serious professional incompetence", "A8", false),
        new("Refer to Professional Standards Team", "A9", false),
        new("Barring by the Secretary of State", "B1", false),
        new("Restricted by the Secretary of State  -  Permitted to work as teacher", "B2A", false),
        new("Restricted by the Secretary of State  -  Not Permitted to work as teacher", "B2B", false),
        new("Prohibited by an Independent Schools Tribunal or Secretary of State", "B3", false),
        new("Employers to contact the Secretary of State", "B4", false),
        new("Prohibited in Scotland or Northern Ireland", "B5", false),
        new("Formerly on List 99", "B6", false),
        new("Prohibited by the Secretary of State  -  failed probation", "C1", true),
        new("Failed induction", "C2", true),
        new(
            "Restricted by the Secretary of State  -  failed probation  -  permitted to carry out specified work for a period equal in length to a statutory induction period only",
            "C3", true),
        new("Ineligible for registration  -  refer to General Teaching Council for Scotland", "D1", false),
        new("Eligible for registration subject to General Teaching Council for Scotland disciplinary order", "D2", false),
        new("Ineligible for registration subject to General Teaching Council for Wales disciplinary order", "D3", false),
        new("Eligible for registration subject to General Teaching Council for Wales disciplinary order", "D4", false),
        new("Ineligible for registration  -  refer to General Teaching Council for Northern Ireland", "D5", false),
        new("Eligible for registration subject to General Teaching Council for Northern Ireland disciplinary order", "D6", false),
        new("Ineligible for registration subject to Independent Schools Tribunal disciplinary drder", "D7", false),
        new("Restricted  -  Deferred for skills test", "E1", false),
        new("Restricted by statutory regulations", "E2", true),
        new("For internal information only - record potentially included in historic GTC data loss", "E3", false),
        new("For internal information only - historic GTC vexatious complainant", "E4", false),
        new("Non Payment of Fee", "F1", false),
        new("A possible matching record was found. Please contact the DBS before employing this person", "G1", true),
        new("Formerly barred by the Independent Safeguarding Authority", "G2", false),
        new("Prohibition by the Secretary of State - misconduct", "T1", true),
        new("Interim prohibition by the Secretary of State", "T2", true),
        new("Prohibition by the Secretary of State  -  deregistered by GTC Scotland", "T3", true),
        new("Prohibition by the Secretary of State  -  refer to the Education Workforce Council, Wales", "T4", true),
        new("Prohibition by the Secretary of State  -  refer to GTC Northern Ireland", "T5", true),
        new("Secretary of State decision- no prohibition", "T6", true),
        new("Council Members never de-register", "Z1", false),
        new("Council Member  -  do not register", "Z2", false)
    ];

    public IReadOnlyCollection<TeacherStatus> TeacherStatuses { get; } =
    [
        new("Early Years Teacher Status", "221", false),
        new("Early Years Trainee", "220", false),
        new("Early Years Professional Status", "222", false),
        new("Qualified Teacher (by virtue of European teaching qualifications)", "223", true),
        new("Qualified teacher: by virtue of achieving international qualified teacher status", "90", true),
        new("Qualified Teacher (by virtue of non-UK teaching qualifications)", "104", true),
        new("Partial qualified teacher status: qualified to teach in SEN establishments", "214", true),
        new(
            "Teacher, other than an uncertificated or supplementary teacher, employed under Regulation 16(3)(a) of the Schools Regulations 1959 or under Regulation 15(3)(a) of the Handicapped Pupils and Special Schools Regulations 1959",
            "10", false),
        new("Qualified Teacher: Assessment Only Route", "100", true),
        new("Qualified Teacher: Teach First Programme (TNP)", "101", false),
        new("Qualified Teacher: Troops to Teach Programme", "102", false),
        new("Qualified Teacher: By virtue of overseas qualifications", "103", true),
        new("Certified teacher under the Code in force before 1 April 1945", "20", false),
        new("Trainee on HEI/SCITT", "200", false),
        new("Trainee on Teach First Programme", "201", false),
        new("Trainee on OTT programme", "202", false),
        new("Trainee on OTT programme (Exempt)", "203", false),
        new("Instructor", "204", false),
        new("OTT: Working under 4 year regulations", "205", false),
        new("Qualified Teacher: Under temporary provision 2005/36/EC", "206", true),
        new("Unknown", "207", false),
        new("Trainee on the Teach First Programme (TNP)", "208", false),
        new("Trainee on the Troops to Teach Programme", "209", false),
        new("Trainee Teacher: HESA", "210", false),
        new("Trainee Teacher", "211", true),
        new("AOR Candidate", "212", true),
        new(
            "Qualified teacher: following at least 1 terms service as a licensed teacher (in respect of 3 yrs OTT), further qualified to teach the deaf or partially hearing impaired under Regulation 15(2) of the Special Schools and Handicapped Pupils",
            "23", false),
        new(
            "Qualified Teacher: following at least 1 school yrs service as a licensed teacher (in respect of those with 2 years or more experience in an independent school), further qualified to teach the deaf or partially hearing impaired under Re",
            "24", false),
        new(
            "Qualified Teacher: Teacher trained/registered in Scotland, further qualified to teach the deaf or partially hearing impaired under Regulation 15(2) of the Special Schools and Handicapped Pupils Regulations 1959.",
            "28", true),
        new(
            "Teacher employed as an uncertificated teacher under Regulation 16 (3)(a) of the Schools Regulations 1959 or under Regulation 15(3)(a) of the Handicapped Pupils and Special Schools Regulations 1959",
            "30", false),
        new(
            "Qualified teacher: following at least one terms service as a licensed teacher (in respect of 3 years overseas trained teachers), further qualified to teach the blind or visually impaired under Regulation 15(2) of the Special Schools and",
            "33", false),
        new(
            "Qualified Teacher: following at least one school years service as a licensed teacher (in respect of those with 2 years or more experience in an independent school), further qualified to teach the blind or visually impaired under Regul",
            "34", false),
        new(
            "Qualified Teacher: Teachers trained/registered in Scotland, further qualified to teach the blind or visually impaired under Regulation 15(2) of the Special Schools and Handicapped Pupils Regulations 1959.",
            "38", false),
        new(
            "Supplementary teacher employed under Regulation 16(3)(a) of the Schools Regulations 1959 or under Regulation 15(3)(a) of the Handicapped Pupils and Special Schools Regulations 1959",
            "40", false),
        new(
            "Person who has completed a course of instruction in the care of young children and whose appointment to the assistant staff of a nursery school or to the staff of a nursery class is approved under Regulation 16(3)(c) of the Schools Regulati",
            "44", false),
        new("Qualified teacher: Following at least one school year's service on the Teach First Programme", "45", false),
        new("Trainee on Registered Teacher Programme", "46", false),
        new("Qualified teacher: Following at least one year's service on the Registered Teacher Programme", "47", true),
        new("Trainee on Graduate Teacher Programme", "48", false),
        new("Qualified teacher: Following at least one term's service on the Graduate Teacher Programme", "49", true),
        new(
            "Person not qualified for employment as a regular teacher whose employment as an Instructor is allowed under Regulation 18 of the Schools Regulations 1959",
            "50", true),
        new("Qualified Teacher (Overseas Trained Teacher needing to complete induction)", "51", false),
        new("Qualified Teacher (Overseas Trained Teacher exempt from induction)", "52", false),
        new("Qualified Teacher (Further Education)", "53", false),
        new("Registered Teacher", "54", false),
        new(
            "Person whose employment is authorised under Regulation 153c of The Handicapped Pupils and Special Schools Regulations 1959 as holder of the Diploma in the Teaching on Mentally Handicapped Children awarded by the Training Council for Teacher",
            "55", false),
        new(
            "Person whose employment is authorised under Regulation 15(3)(c) of The Handicapped Pupils and Special Schools Regulations 1959 as holder of the Declaration of Recognition of Experience awarded by the Training Council for Teachers of the Men",
            "56", false),
        new("Person whose employment is authorised under Regulation 16A of the Handicapped Pupils and Special Schools Regulations 1959", "57", false),
        new("Qualified teacher: TCMH  3 year,s experience.", "58", false),
        new("Licenced teacher who has withdrawn and licence is cancelled", "59", false),
        new("Student teacher whose employment is approved under Regulation 17 (1) of the Schools Regulations 1959", "60", false),
        new("Licensed Teacher", "61", false),
        new("Qualified Teacher: following at least 2 year's service as a licensed teacher", "62", false),
        new("Qualified Teacher: following at least one term's service as a licensed teacher (in respect of 3 year's overseas trained teachers)", "63", false),
        new(
            "Qualified Teacher: following at least one school year's service as a licensed teacher (in respect of those with 2 year's or more experience in an independent school)",
            "64", false),
        new(
            "Qualified Teacher: following at least one school year's service as a licensed teacher (in respect of those with 2 year's or more experience in further education)",
            "65", false),
        new(
            "Qualified Teacher: following at least one schoolyear's service as a licensed teacher (in respect of those with 2 year's or more experience in the educational services of the Armed Forces)",
            "66", false),
        new("Qualified Teacher: Under the EC Directive", "67", true),
        new("Qualified Teacher: Teachers trained/registered in Scotland", "68", true),
        new("Qualified Teacher: Teachers trained/recognised by the Department of Education for Northern Ireland (DENI)", "69", true),
        new("FAILED Licensed Teacher", "70", false),
        new("Qualified teacher (trained)", "71", true),
        new("Qualified teacher (graduate non-trained)", "72", false),
        new("Qualified teacher (by virtue of long service)", "73", false),
        new(
            "Qualified teacher following at least one years employment as a licensed teacher with at least two years previous experience as an instructor in a maintained school (from 1 September 1991)",
            "74", false),
        new("Authorised teacher (from 1 September 1991)", "75", false),
        new(
            "Qualified teacher following at least one terms employment as an authorized teacher with at least one years teaching experience (from 1 september 1991)",
            "76", false),
        new("Qualified teacher following a school centred Initial Teacher Training course (SCITT)", "77", false),
        new("Qualified teacher following successful completion of a period of Registration in a CTC or CCTA", "78", false),
        new("Qualified teacher (by virtue of other qualifications)", "79", false),
        new("Qualified Teacher (under the Flexible Post-graduate route)", "80", false),
        new(
            "Qualified teacher (trained) further qualified to teach the deaf or partially hearing under Regulation 15(2) of the Special Schools and Handicapped Pupils Regulations 1959",
            "81", false),
        new(
            "Qualified teacher (graduate non-trained) further qualified to teach the deaf or partially hearing under Regulation 15(2) of the Special Schools and Handicapped Pupils Regulations 1959",
            "82", false),
        new(
            "Qualified teacher (by virtue of long service) further qualified to teach the deaf or partially hearing under Regulation 15(2) of the Special Schools and Handicapped Pupils Regulations 1959",
            "83", false),
        new("Qualified teacher of children with Multi-sensory Impairments (from 1 June 1991)", "85", false),
        new(
            "Qualified teacher (under the EC Directive) further qualified to teach the deaf or partially hearing under Regulation 15(2) of the Special Schools and Handicapped Pupils Regulations 1959",
            "87", true),
        new(
            "Qualified teacher (by virtue of other qualifications) further qualified to teach the deaf or partially hearing under Regulation 15(2) of the Special Schools and Handicapped Pupils Regulations",
            "89", false),
        new(
            "Qualified teacher (trained) further qualified to teach the blind under Regulation 15(2) of the Special Schools and Handicapped Pupils Regulations 1959",
            "91", false),
        new(
            "Qualified teacher (graduate non-trained) further qualified to teach the blind under Regulation 15(2) of the Special Schools and Handicapped Pupils Regulations 1959",
            "92", false),
        new(
            "Qualified teacher (by virtue of long service) further qualified to teach the blind under Regulation 15(2) of the Special Schools and Handicapped Pupils Regulations 1959",
            "93", false),
        new(
            "Qualified teacher (under the EC Directive) further qualifed to teach the blind under Regulation 15(2) of the Special Schools and Handicapped Pupils Regulations 1959",
            "97", false),
        new(
            "Qualified teacher (by virtue of other qualifications) further qualified to teach the blind under Regulation 15(2) of the Special Schools and Handicapped Pupils Regulations 1959 a maintained school.",
            "99", false),
        new("QTS Awarded in error", "999", false),
        new("Qualified Teacher: QTS awarded in Wales", "213", true)
    ];

    public IReadOnlyCollection<EarlyYearsStatus> EarlyYearsStatuses { get; } =
    [
        new("Early Years Trainee", "220", true),
        new("Early Years Teacher Status", "221", true),
        new("Early Years Professional Status", "222", true)
    ];

    public IReadOnlyCollection<MqEstablishment> GetAllMqEstablishments(bool activeOnly = true) =>
        MqEstablishments.Where(m => !activeOnly || m.Active).AsReadOnly();

    public MqEstablishment GetMqEstablishmentByValue(string value) =>
        MqEstablishments.Single(m => m.Value == value);

    public IReadOnlyCollection<MqSpecialism> GetAllMqSpecialisms(bool activeOnly = true) =>
        MqSpecialisms.Where(s => !activeOnly || s.Active).AsReadOnly();

    public MqSpecialism GetMqSpecialismByValue(string value) =>
        MqSpecialisms.Single(s => s.Value == value);

    public IReadOnlyCollection<SanctionCode> GetAllSanctionCodes(bool activeOnly = true) =>
        SanctionCodes.Where(s => !activeOnly || s.Active).AsReadOnly();

    public SanctionCode GetSanctionCodeByValue(string value) =>
        SanctionCodes.Single(s => s.Value == value);

    public IReadOnlyCollection<TeacherStatus> GetAllTeacherStatuses(bool activeOnly = true) =>
        TeacherStatuses.Where(s => !activeOnly || s.Active).AsReadOnly();

    public TeacherStatus GetTeacherStatusByValue(string value) =>
        TeacherStatuses.Single(s => s.Value == value);

    public IReadOnlyCollection<EarlyYearsStatus> GetAllEarlyYearsStatuses(bool activeOnly = true) =>
        EarlyYearsStatuses.Where(s => !activeOnly || s.Active).AsReadOnly();

    public EarlyYearsStatus GetEarlyYearsStatusByValue(string value) =>
        EarlyYearsStatuses.Single(s => s.Value == value);

    public record MqEstablishment(string Name, string Value, bool Active);

    public record MqSpecialism(string Name, string Value, bool Active);

    public record SanctionCode(string Name, string Value, bool Active);

    public record TeacherStatus(string Name, string Value, bool Active);

    public record EarlyYearsStatus(string Name, string Value, bool Active);
}
