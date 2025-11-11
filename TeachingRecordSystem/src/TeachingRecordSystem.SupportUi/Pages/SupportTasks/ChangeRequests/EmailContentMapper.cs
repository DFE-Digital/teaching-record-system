using static TeachingRecordSystem.SupportUi.Pages.SupportTasks.ChangeRequests.EditChangeRequest.RejectModel;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ChangeRequests;

public static class EmailContentMapper
{
    public static string EmailReason(this CaseRejectionReasonOption reason) => reason switch
    {
        CaseRejectionReasonOption.RequestAndProofDontMatch => "This is because the proof you provided did not match your request.",
        CaseRejectionReasonOption.WrongTypeOfDocument => "This is because you provided the wrong type of document.",
        CaseRejectionReasonOption.ImageQuality => "This is because the image you provided was not clear enough.",
        _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, null)
    };
}
