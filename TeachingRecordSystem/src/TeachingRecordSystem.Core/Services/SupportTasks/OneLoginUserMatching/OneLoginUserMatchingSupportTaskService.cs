using TeachingRecordSystem.Core.Services.OneLogin;

namespace TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserMatching;

public partial class OneLoginUserMatchingSupportTaskService(
    SupportTaskService supportTaskService,
    OneLoginService oneLoginService);
