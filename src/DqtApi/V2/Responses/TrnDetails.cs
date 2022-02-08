using System.Collections.Generic;

namespace DqtApi.V2.Responses
{
    public class TrnDetails
    {
        public string Trn { get; set; }
        public List<string> EmailAddresses { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DateOfBirth { get; set; }
        public string Nino { get; set; }
        public string Uid { get; set; }
    }
}
