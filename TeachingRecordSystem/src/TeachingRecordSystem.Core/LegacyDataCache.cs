namespace TeachingRecordSystem.Core;

public class LegacyDataCache
{
    private LegacyDataCache() { }

    public static LegacyDataCache Instance { get; } = new();

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

    public IReadOnlyCollection<SanctionCode> GetAllSanctionCodes(bool activeOnly = true) =>
        SanctionCodes.Where(s => !activeOnly || s.Active).AsReadOnly();

    public SanctionCode GetSanctionCodeByValue(string value) =>
        SanctionCodes.Single(s => s.Value == value);

    public record SanctionCode(string Name, string Value, bool Active);
}
