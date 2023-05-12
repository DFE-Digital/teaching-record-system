#nullable disable
using System.Text.Json.Serialization;
using QualifiedTeachersApi.V2.ApiModels;

namespace QualifiedTeachersApi.V1.Responses;

public class Qualification
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("date_awarded")]
    public DateTime? DateAwarded { get; set; }

    [JsonPropertyName("he_qualification_name")]
    public string HeQualificationName { get; set; }

    [JsonPropertyName("he_subject1")]
    public string Subject1 { get; set; }

    [JsonPropertyName("he_subject2")]
    public string Subject2 { get; set; }

    [JsonPropertyName("he_subject3")]
    public string Subject3 { get; set; }

    [JsonPropertyName("he_subject1_code")]
    public string Subject1Code { get; set; }

    [JsonPropertyName("he_subject2_code")]
    public string Subject2Code { get; set; }

    [JsonPropertyName("he_subject3_code")]
    public string Subject3Code { get; set; }

    [JsonPropertyName("class")]
    public ClassDivision? ClassDivision { get; set; }
}
