﻿namespace DqtApi
{
    public static class CacheKeys
    {
        public static object GetCountryKey(string code) => $"country:{code}";

        public static object GetEarlyYearsStatusKey(string code) => $"early_years_status:{code}";

        public static object GetHeQualificationKey(string name) => $"he_qualification:{name}";

        public static object GetHeSubjectKey(string name) => $"he_subject:{name}";

        public static object GetIttProviderOrganizationByUkprnKey(string ukprn) => $"organization_itt_provider:{ukprn}";

        public static object GetIttSubjectKey(string name) => $"itt_subject:{name}";

        public static object GetIttQualificationKey(string code) => $"itt_qualification:{code}";

        public static object GetOrganizationByUkprnKey(string ukprn) => $"organization:{ukprn}";

        public static object GetTeacherStatusKey(string code) => $"teacher_status:{code}";
    }
}
