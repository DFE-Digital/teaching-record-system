namespace TeachingRecordSystem.Core.Dqt;

public class SanctionTextLookup
{
    private readonly IDictionary<string, string> sanctionDefaultText = new Dictionary<string, string>();

    public SanctionTextLookup()
    {
        sanctionDefaultText.Add("A13", "Can only teach in maintained schools, pupil referral units and non-maintained special schools subject to the conditions of the sanction. Contact the Teaching Regulation Agency (TRA) on 0207 593 5393 to confirm the current status of the order.");
        sanctionDefaultText.Add("A14", "Can only teach in maintained schools, pupil referral units and non-maintained special schools subject to the conditions of the sanction. Contact the Teaching Regulation Agency (TRA) on 0207 593 5393 to confirm the current status of the order.");
        sanctionDefaultText.Add("A18", "Conviction of a relevant offence. [01/01/2001]. Can only teach in maintained schools, pupil referral units and non-maintained special schools subject to the conditions of the sanction. Contact the Teaching Regulation Agency (TRA) on 0207 593 5393 to confirm the current status of the order.");
        sanctionDefaultText.Add("A19", "Can only teach in maintained schools, pupil referral units and non-maintained special schools subject to the conditions of the sanction. Contact the Teaching Regulation Agency (TRA) on 0207 593 5393 to confirm the current status of the order.");
        sanctionDefaultText.Add("A1A", "Unacceptable professional conduct. [01/01/2001]. Cannot teach in any maintained school, pupil referral unit or non-maintained special school.");
        sanctionDefaultText.Add("A1B", "Unacceptable professional conduct. [01/01/2001]. Will be reviewed on [01/01/2022]. Cannot teach in any school, including sixth-form colleges, relevant youth accommodation and children’s homes.");
        sanctionDefaultText.Add("A2", "Can only teach in maintained schools, pupil referral units and non-maintained special schools subject to the conditions of the sanction. Contact the Teaching Regulation Agency (TRA) on 0207 593 5393 to confirm the current status of the order.");
        sanctionDefaultText.Add("A20", "Can only teach in maintained schools, pupil referral units and non-maintained special schools subject to the conditions of the sanction. Contact the Teaching Regulation Agency (TRA) on 0207 593 5393 to confirm the current status of the order.");
        sanctionDefaultText.Add("A21A", "Conviction of a relevant offence. [01/01/2001]. Cannot teach in any maintained school, pupil referral unit or non-maintained special school.");
        sanctionDefaultText.Add("A21B", "Conviction of a relevant offence. [01/01/2001]. Will be reviewed on [01/01/2022].");
        sanctionDefaultText.Add("A23", "Can only teach in maintained schools, pupil referral units and non-maintained special schools subject to the conditions of the sanction. Contact the Teaching Regulation Agency (TRA) on 0207 593 5393 to confirm the current status of the order.");
        sanctionDefaultText.Add("A24", "Can only teach in maintained schools, pupil referral units and non-maintained special schools subject to the conditions of the sanction. Contact the Teaching Regulation Agency (TRA) on 0207 593 5393 to confirm the current status of the order.");
        sanctionDefaultText.Add("A25A", "Breach of conditions. [01/01/2001]. Cannot teach in any maintained school, pupil referral unit or non-maintained special school.");
        sanctionDefaultText.Add("A25B", "Breach of conditions. [01/01/2001]. Will be reviewed on [01/01/2022]. Cannot teach in any school, including sixth-form colleges, relevant youth accommodation and children’s homes.");
        sanctionDefaultText.Add("A3", "Unacceptable professional conduct. [01/01/2001]. Can only teach in maintained schools, pupil referral units and non-maintained special schools subject to the conditions of the sanction. Contact the Teaching Regulation Agency (TRA) on 0207 593 5393 to confirm the current status of the order.");
        sanctionDefaultText.Add("A5A", "Serious professional incompetence. [01/01/2001]. Cannot teach in any maintained school, pupil referral unit or non-maintained special school.");
        sanctionDefaultText.Add("A5B", "SSerious professional incompetence. [01/01/2001]. Will be reviewed on [01/01/2022]. Cannot teach in any school, including sixth-form colleges, relevant youth accommodation and children’s homes.");
        sanctionDefaultText.Add("A6", "Can only teach in maintained schools, pupil referral units and non-maintained special schools subject to the conditions of the sanction. Contact the Teaching Regulation Agency (TRA) on 0207 593 5393 to confirm the current status of the order.");
        sanctionDefaultText.Add("A7", "Serious professional incompetence.[01/01/2001]. Can only teach in maintained schools, pupil referral units and non-maintained special schools subject to the conditions of the sanction. Contact the Teaching Regulation Agency (TRA) on 0207 593 5393 to confirm the current status of the order.");
        sanctionDefaultText.Add("B3", "[01/01/2001]. Cannot teach in any school, including sixth-form colleges, relevant youth accommodation and children’s homes.");
        sanctionDefaultText.Add("C1", "Failed probation. [01/02/2001]. Cannot teach in any maintained school, pupil referral unit or non-maintained special school.");
        sanctionDefaultText.Add("C3", "Failed probation [01/01/2001]. Can carry out specified work for the same amount of time as a statutory induction period.");
        sanctionDefaultText.Add("G1", "Contact DBS for more details. [Contact details]");
        sanctionDefaultText.Add("T2", "Interim prohibition. [01/01/2001]. Investigation ongoing. Cannot teach in any school, including sixth-form colleges, relevant youth accommodation and children’s homes.");
        sanctionDefaultText.Add("T3", "Deregistered by the General Teaching Council for Scotland (GTCS). [01/01/2001]. Cannot teach in any maintained school, pupil referral unit or non-maintained special school.");
        sanctionDefaultText.Add("T4", "Contact the Education Workforce Council for more details.");
        sanctionDefaultText.Add("T5", "Contact the General Teaching Council for Northern Ireland (GTCNI) for more details.");
    }

    public string? GetSanctionDefaultText(string sanctionCode)
    {
        sanctionDefaultText.TryGetValue(sanctionCode, out string? defaultText);
        return defaultText;
    }
}
