using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.Core.Services.TrnRequests;

namespace TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserMatching;

public partial class OneLoginUserMatchingSupportTaskService(
    SupportTaskService supportTaskService,
    OneLoginService oneLoginService,
    TrnRequestService trnRequestService);
