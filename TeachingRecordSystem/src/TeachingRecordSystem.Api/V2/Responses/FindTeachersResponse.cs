namespace TeachingRecordSystem.Api.V2.Responses;

public class FindTeachersResponse
{
    public required IEnumerable<FindTeacherResult> Results { get; set; }
}
