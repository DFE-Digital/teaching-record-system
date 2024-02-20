using System.Diagnostics.CodeAnalysis;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class MandatoryQualificationProvider
{
    public required Guid MandatoryQualificationProviderId { get; init; }
    public required string Name { get; set; }

    public static bool TryMapFromDqtMqEstablishment(
        dfeta_mqestablishment? mqestablishment,
        [NotNullWhen(true)] out MandatoryQualificationProvider? provider)
    {
        if (mqestablishment is null)
        {
            provider = null;
            return false;
        }

        return TryMapFromDqtMqEstablishmentValue(mqestablishment.dfeta_Value, out provider);
    }

    public static bool TryMapFromDqtMqEstablishmentValue(
        string mqestablishmentValue,
        [NotNullWhen(true)] out MandatoryQualificationProvider? provider)
    {
        switch (mqestablishmentValue)
        {
            case "963":  // University of Oxford/Oxford Polytechnic
                provider = All.Single(p => p.Name == "University of Oxford/Oxford Polytechnic");
                return true;
            case "957":  // University of Edinburgh
                provider = All.Single(p => p.Name == "University of Edinburgh");
                return true;
            case "150":  // Postgraduate Diploma in Deaf Education, University of Manchester, School of Psychological Sciences
            case "961":  // University of Manchester
                provider = All.Single(p => p.Name == "University of Manchester");
                return true;
            case "210":  // Postgraduate Diploma in Multi-Sensory Impairment and Deafblindness, University of Birmingham, School of Education
            case "180":  // BPhil in Multi-Sensory Impairment and Deafblindness, University of Birmingham, School of Education
            case "160":  // BPhil in Education (Special Education: Hearing Impairment), University of Birmingham, School of Education
            case "120":  // Postgraduate Diploma in Education (Special Education: Hearing Impairment), University of Birmingham, School of Education
            case "20":  // BPhil for Teachers of Children with a Visual Impairment, University of Birmingham, School of Education
            case "30":  // Postgraduate Diploma for Teachers of Children with Visual Impairment, University of Birmingham, School of Education
            case "955":  // University of Birmingham
                provider = All.Single(p => p.Name == "University of Birmingham");
                return true;
            case "956":  // University of Cambridge
                provider = All.Single(p => p.Name == "University of Cambridge");
                return true;
            case "964":  // Liverpool John Moores University
                provider = All.Single(p => p.Name == "Liverpool John Moores University");
                return true;
            case "959":  // University of Leeds
                provider = All.Single(p => p.Name == "University of Leeds");
                return true;
            case "962":  // University of Newcastle-upon-Tyne
                provider = All.Single(p => p.Name == "University of Newcastle-upon-Tyne");
                return true;
            case "90":  // Masters Level: Mandatory Qualification for Teachers of Children with Visual Impairment, University of Plymouth, Faculty of Education, in partnership with the Sensory Consortium
            case "965":  // Plymouth University
                provider = All.Single(p => p.Name == "University of Plymouth");
                return true;
            case "140":  // Postgraduate Diploma (Education of Deaf Children), University of Hertfordshire
            case "958":  // University of Hertfordshire
                provider = All.Single(p => p.Name == "University of Hertfordshire");
                return true;
            case "960":  // University of London
            case "50":  // Graduate Diploma in Special and Inclusive Education: Disabilities of Sight, University of London Institute of Education
                provider = All.Single(p => p.Name == "University of London");
                return true;
            case "951":  // Bristol Polytechnic
                provider = All.Single(p => p.Name == "Bristol Polytechnic");
                return true;
            case "954":  // University College, Swansea
                provider = All.Single(p => p.Name == "University College, Swansea");
                return true;
            default:
                provider = null;
                return false;
        }
    }

    public static MandatoryQualificationProvider GetById(Guid id) => All.Single(p => p.MandatoryQualificationProviderId == id);

    public static MandatoryQualificationProvider[] All { get; } =
    [
        new MandatoryQualificationProvider()
        {
            MandatoryQualificationProviderId = new Guid("e28ea41d-408d-4c89-90cc-8b9b04ac68f5"),
            Name = "University of Birmingham"
        },
        new MandatoryQualificationProvider()
        {
            MandatoryQualificationProviderId = new Guid("89f9a1aa-3d68-4985-a4ce-403b6044c18c"),
            Name = "University of Leeds"
        },
        new MandatoryQualificationProvider()
        {
            MandatoryQualificationProviderId = new Guid("aa5c300e-3b7c-456c-8183-3520b3d55dca"),
            Name = "University of Manchester"
        },
        new MandatoryQualificationProvider()
        {
            MandatoryQualificationProviderId = new Guid("f417e73e-e2ad-40eb-85e3-55865be7f6be"),
            Name = "Mary Hare School / University of Hertfordshire"
        },
        new MandatoryQualificationProvider()
        {
            MandatoryQualificationProviderId = new Guid("fbf22e04-b274-4c80-aba8-79fb6a7a32ce"),
            Name = "University of Edinburgh"
        },
        new MandatoryQualificationProvider()
        {
            MandatoryQualificationProviderId = new Guid("26204149-349c-4ad6-9466-bb9b83723eae"),
            Name = "Liverpool John Moores University"
        },
        new MandatoryQualificationProvider()
        {
            MandatoryQualificationProviderId = new Guid("0c30f666-647c-4ea8-8883-0fc6010b56be"),
            Name = "University of Oxford/Oxford Polytechnic"
        },
        new MandatoryQualificationProvider()
        {
            MandatoryQualificationProviderId = new Guid("d0e6d54c-5e90-438a-945d-f97388c2b352"),
            Name = "University of Cambridge"
        },
        new MandatoryQualificationProvider()
        {
            MandatoryQualificationProviderId = new Guid("aec32252-ef25-452e-a358-34a04e03369c"),
            Name = "University of Newcastle-upon-Tyne"
        },
        new MandatoryQualificationProvider()
        {
            MandatoryQualificationProviderId = new Guid("d9ee7054-7fde-4cfd-9a5e-4b99511d1b3d"),
            Name = "University of Plymouth"
        },
        new MandatoryQualificationProvider()
        {
            MandatoryQualificationProviderId = new Guid("707d58ca-1953-413b-9a46-41e9b0be885e"),
            Name = "University of Hertfordshire"
        },
        new MandatoryQualificationProvider()
        {
            MandatoryQualificationProviderId = new Guid("3fc648a7-18e4-49e7-8a4b-1612616b72d5"),
            Name = "University of London"
        },
        new MandatoryQualificationProvider()
        {
            MandatoryQualificationProviderId = new Guid("374dceb8-8224-45b8-b7dc-a6b0282b1065"),
            Name = "Bristol Polytechnic"
        },
        new MandatoryQualificationProvider()
        {
            MandatoryQualificationProviderId = new Guid("d4fc958b-21de-47ec-9f03-36ae237a1b11"),
            Name = "University College, Swansea"
        },
    ];
}
