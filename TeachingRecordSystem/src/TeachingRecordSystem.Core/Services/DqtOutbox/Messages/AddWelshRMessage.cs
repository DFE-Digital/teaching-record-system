using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachingRecordSystem.Core.Services.DqtOutbox.Messages;

public class AddWelshRMessage
{
    public required Guid PersonId { get; set; }
    public required DateOnly? AwardedDate { get; set; }
    public required List<Guid> Subjects { get; set; }
    public required Guid? TrainingProviderId { get; set; }
    public required DateOnly? TrainingStartDate { get; set; }
    public required DateOnly? TrainingEndDate { get; set; }
    public required int? TrainingAgeSpecialismRangeFrom { get; set; }
    public required int? TrainingAgeSpecialismRangeTo { get; set; }
    public required string? TrainingCountryId { get; set; }
    public required TrainingAgeSpecialismType? TrainingAgeSpecialismType { get; set; }
}

