namespace DqtApi
{
    public static class CacheKeys
    {
        public static object GetCountryKey(string code) => $"country:{code}";

        public static object GetEarlyYearsStatusKey(string code) => $"early_years_status:{code}";

        public static object GetHeSubjectKey(string name) => $"he_subject:{name}";

        public static object GetIttSubjectKey(string name) => $"itt_subject:{name}";

        public static object GetOrganizationByUkprnKey(string ukprn) => $"organization:{ukprn}";
    }
}
