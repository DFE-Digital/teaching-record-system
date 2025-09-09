namespace TeachingRecordSystem.Core;

public class LegacyDataCache
{
    private LegacyDataCache() { }

    public static LegacyDataCache Instance { get; } = new();

    public IReadOnlyCollection<MqEstablishment> MqEstablishments =
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

    public IReadOnlyCollection<MqSpecialism> MqSpecialisms =
    [
        new("Deaf education", "Deaf education", true),
        new("N/A", "N/A", true),
        new("Auditory Impairment", "Auditory", false),
        new("Hearing", "Hearing", true),
        new("Multi_Sensory Impairment", "Multi-Sensory", true),
        new("Visual Impairment", "Visual", true)
    ];

    public IReadOnlyCollection<SanctionCode> SanctionCodes =
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
        new("Restricted by the Secretary of State  -  failed probation  -  permitted to carry out specified work for a period equal in length to a statutory induction period only", "C3", true),
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

    public record MqEstablishment(string Name, string Value, bool Active);

    public record MqSpecialism(string Name, string Value, bool Active);

    public record SanctionCode(string Name, string Value, bool Active);
}
