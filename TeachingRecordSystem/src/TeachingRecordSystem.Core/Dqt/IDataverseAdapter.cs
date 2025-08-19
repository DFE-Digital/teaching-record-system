#nullable disable

namespace TeachingRecordSystem.Core.Dqt;

public interface IDataverseAdapter
{
    Task<dfeta_qualification[]> GetQualificationsForTeacherAsync(
        Guid teacherId,
        string[] columnNames,
        string[] heQualificationColumnNames = null,
        string[] heSubjectColumnNames = null);
}
