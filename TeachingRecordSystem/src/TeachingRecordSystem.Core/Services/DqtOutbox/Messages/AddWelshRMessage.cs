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
}

