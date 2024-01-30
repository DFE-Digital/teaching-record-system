namespace TeachingRecordSystem.Core.Dqt.Models;

public static class AttributeConstraints
{
    public static class Contact
    {
        public const int FirstNameMaxLength = 100;
        public const int MiddleNameMaxLength = 100;
        public const int LastNameMaxLength = 100;
        public const int EMailAddress1MaxLength = 100;
        public const int Address1_Line1MaxLength = 250;
        public const int Address1_Line2MaxLength = 250;
        public const int Address1_Line3MaxLength = 250;
        public const int Address1_CountyMaxLength = 50;
        public const int Address1_CityMaxLength = 80;
        public const int Address1_PostalCodeLength = 20;
        public const int Address1_CountryMaxLength = 80;
        public const int SlugId_MaxLength = 150;
        public const int NationalInsuranceNumber_MaxLength = 9;
    }
}
